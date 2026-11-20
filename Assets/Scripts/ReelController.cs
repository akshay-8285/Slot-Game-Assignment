using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ReelController controls an individual reel in the slot machine.
/// Responsibilities:
/// 1. Initializes symbol positions and starting sprites.
/// 2. Performs smooth vertical downward spinning using a Coroutine.
/// 3. Seamlessly wraps symbols that scroll past the bottom back to the top.
/// 4. Decelerates and smoothly glides to a stop on the exact RNG target symbol.
/// 5. Adds a gentle physical bounce for authentic mechanical slot feel.
/// </summary>
public class ReelController : MonoBehaviour
{
    [Header("Reel Configuration")]
    [Tooltip("The vertical distance between centers of consecutive symbols (in pixels)")]
    [SerializeField] private float symbolSpacing = 96f;

    [Tooltip("Normal spinning speed in pixels per second")]
    [SerializeField] private float maxSpinSpeed = 1600f;

    [Tooltip("Bounce distance in pixels when the reel stops")]
    [SerializeField] private float bounceAmount = 12f;

    [Tooltip("Duration of the stop bounce animation in seconds")]
    [SerializeField] private float bounceDuration = 0.18f;

    [Header("UI Symbol Image References")]
    [Tooltip("The list of Image components on this reel strip (5 images: buffers + visible rows)")]
    [SerializeField] private Image[] symbolImages;

    // --- Runtime State Variables ---
    // Tracks whether this reel is currently in motion
    private bool isSpinning = false;
    public bool IsSpinning => isSpinning;

    // The symbol that will land on the center payline when stopping
    private SymbolData targetCenterSymbol;

    // The symbol data currently sitting on the center payline
    private SymbolData currentCenterSymbol;
    public SymbolData CurrentCenterSymbol => currentCenterSymbol;

    // Total height of the looping reel strip (5 symbols * 96px = 480px)
    private float totalLoopHeight;

    // Flag indicating the reel has received the command to stop
    private bool stopRequested = false;

    private void Awake()
    {
        totalLoopHeight = symbolImages.Length * symbolSpacing;
    }

    /// <summary>
    /// Initializes the reel with symbols at startup.
    /// Sets initial vertical positions and random symbols.
    /// </summary>
    /// <param name="availableSymbols">Array of symbols available in the game</param>
    public void InitializeReel(SymbolData[] availableSymbols)
    {
        if (symbolImages == null || symbolImages.Length == 0 || availableSymbols == null || availableSymbols.Length == 0)
        {
            Debug.LogError($"[ReelController] Missing symbols or image references on {gameObject.name}!");
            return;
        }

        totalLoopHeight = symbolImages.Length * symbolSpacing;

        // Position the 5 images vertically centered around Y = 0
        // Index 0: -192 (bottom buffer)
        // Index 1:  -96 (visible bottom)
        // Index 2:    0 (visible CENTER payline)
        // Index 3:  +96 (visible top)
        // Index 4: +192 (top buffer)
        float startY = -((symbolImages.Length - 1) / 2f) * symbolSpacing;

        for (int i = 0; i < symbolImages.Length; i++)
        {
            RectTransform rect = symbolImages[i].rectTransform;
            float yPos = startY + (i * symbolSpacing);
            rect.anchoredPosition = new Vector2(0f, yPos);

            // Assign a random starting symbol
            int randomIndex = Random.Range(0, availableSymbols.Length);
            symbolImages[i].sprite = availableSymbols[randomIndex].symbolSprite;

            // Track the initial center symbol (Index 2)
            if (i == 2)
            {
                currentCenterSymbol = availableSymbols[randomIndex];
            }
        }
    }

    /// <summary>
    /// Starts the reel spinning coroutine.
    /// </summary>
    /// <param name="availableSymbols">Symbol pool used to cycle random symbols during spin</param>
    public void StartSpin(SymbolData[] availableSymbols)
    {
        if (isSpinning) return;

        stopRequested = false;
        StartCoroutine(SpinRoutine(availableSymbols));
    }

    /// <summary>
    /// Signals the reel to begin stopping and land on the given target symbol.
    /// </summary>
    /// <param name="target">The symbol determined by the RNG for this reel</param>
    public void StopSpin(SymbolData target)
    {
        targetCenterSymbol = target;
        stopRequested = true;
    }

    /// <summary>
    /// Coroutine that handles continuous spinning, deceleration, seamless snap, and bounce.
    /// </summary>
    private IEnumerator SpinRoutine(SymbolData[] availableSymbols)
    {
        isSpinning = true;
        float currentSpeed = 0f;
        float accelerationRate = maxSpinSpeed * 4f; // Quick smooth start

        // PHASE 1: Accelerate to full spinning speed and spin until stop is requested
        while (!stopRequested)
        {
            if (currentSpeed < maxSpinSpeed)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpinSpeed, accelerationRate * Time.deltaTime);
            }

            MoveSymbolsDown(currentSpeed * Time.deltaTime, availableSymbols);
            yield return null;
        }

        // PHASE 2: Smooth deceleration
        float decelerationTime = 0.35f;
        float elapsed = 0f;
        float speedAtStopRequest = currentSpeed;

        while (elapsed < decelerationTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / decelerationTime;
            // Smooth ease-out deceleration
            currentSpeed = Mathf.Lerp(speedAtStopRequest, 250f, Mathf.SmoothStep(0f, 1f, t));

            MoveSymbolsDown(currentSpeed * Time.deltaTime, availableSymbols);
            yield return null;
        }

        // PHASE 3: Seamless alignment glide to exact center (Y = 0)
        // Record current offset from center to eliminate any sudden visual jumps
        float startBaseY = -((symbolImages.Length - 1) / 2f) * symbolSpacing;
        float currentCenterY = symbolImages[2].rectTransform.anchoredPosition.y;

        // Assign the RNG target symbol to the center payline slot (Index 2)
        currentCenterSymbol = targetCenterSymbol;
        symbolImages[2].sprite = targetCenterSymbol.symbolSprite;

        // Assign valid adjacent symbols for realistic appearance
        for (int i = 0; i < symbolImages.Length; i++)
        {
            if (i != 2)
            {
                int randIndex = Random.Range(0, availableSymbols.Length);
                symbolImages[i].sprite = availableSymbols[randIndex].symbolSprite;
            }
        }

        // Smoothly glide the remaining fractional distance into grid alignment
        float snapElapsed = 0f;
        float snapDuration = 0.12f;
        while (snapElapsed < snapDuration)
        {
            snapElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(snapElapsed / snapDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < symbolImages.Length; i++)
            {
                float targetGridY = startBaseY + (i * symbolSpacing);
                float fromY = targetGridY + currentCenterY;
                float newY = Mathf.Lerp(fromY, targetGridY, smoothT);
                symbolImages[i].rectTransform.anchoredPosition = new Vector2(0f, newY);
            }
            yield return null;
        }

        // Enforce exact mathematical positions
        for (int i = 0; i < symbolImages.Length; i++)
        {
            float targetGridY = startBaseY + (i * symbolSpacing);
            symbolImages[i].rectTransform.anchoredPosition = new Vector2(0f, targetGridY);
        }

        // PHASE 4: Gentle physical bounce (mechanical slot feel)
        yield return StartCoroutine(BounceRoutine(startBaseY));

        isSpinning = false;
    }

    /// <summary>
    /// Moves all symbol images downwards by deltaY and wraps symbols that exit the bottom
    /// back to the top of the strip.
    /// </summary>
    private void MoveSymbolsDown(float deltaY, SymbolData[] availableSymbols)
    {
        float bottomLimit = -((symbolImages.Length / 2f) * symbolSpacing) - (symbolSpacing * 0.5f);

        for (int i = 0; i < symbolImages.Length; i++)
        {
            RectTransform rect = symbolImages[i].rectTransform;
            Vector2 pos = rect.anchoredPosition;
            pos.y -= deltaY;

            // If symbol moved below the bottom limit, wrap it to the top
            if (pos.y < bottomLimit)
            {
                pos.y += totalLoopHeight;
                // Give the wrapped symbol a new random graphic
                int randIndex = Random.Range(0, availableSymbols.Length);
                symbolImages[i].sprite = availableSymbols[randIndex].symbolSprite;
            }

            rect.anchoredPosition = pos;
        }
    }

    /// <summary>
    /// Small elastic bounce animation on stop.
    /// Overshoots slightly downwards then springs back to center.
    /// </summary>
    private IEnumerator BounceRoutine(float baseStartY)
    {
        float halfBounce = bounceDuration * 0.5f;
        float timer = 0f;

        // Move down by bounceAmount
        while (timer < halfBounce)
        {
            timer += Time.deltaTime;
            float progress = timer / halfBounce;
            float currentOffset = Mathf.Lerp(0f, -bounceAmount, progress);

            ApplyVerticalOffset(baseStartY, currentOffset);
            yield return null;
        }

        timer = 0f;
        // Spring back to 0
        while (timer < halfBounce)
        {
            timer += Time.deltaTime;
            float progress = timer / halfBounce;
            float currentOffset = Mathf.Lerp(-bounceAmount, 0f, progress);

            ApplyVerticalOffset(baseStartY, currentOffset);
            yield return null;
        }

        // Final perfect alignment
        ApplyVerticalOffset(baseStartY, 0f);
    }

    /// <summary>
    /// Sets all slot positions relative to the base positions plus an offset.
    /// </summary>
    private void ApplyVerticalOffset(float baseStartY, float offset)
    {
        for (int i = 0; i < symbolImages.Length; i++)
        {
            float targetY = baseStartY + (i * symbolSpacing) + offset;
            symbolImages[i].rectTransform.anchoredPosition = new Vector2(0f, targetY);
        }
    }
}
