using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Controls an individual reel: spinning, wrapping symbols, and stopping on the target symbol.
public class ReelController : MonoBehaviour
{
    [Header("Reel Configuration")]
    [SerializeField] private float symbolSpacing = 96f;
    [SerializeField] private float maxSpinSpeed = 1600f;
    [SerializeField] private float bounceAmount = 12f;
    [SerializeField] private float bounceDuration = 0.18f;

    [Header("UI Symbol Image References")]
    [SerializeField] private Image[] symbolImages;

    private bool isSpinning = false;
    public bool IsSpinning => isSpinning;

    // The symbol to land on the center payline
    private SymbolData targetCenterSymbol;

    // The symbol currently in the center payline
    private SymbolData currentCenterSymbol;
    public SymbolData CurrentCenterSymbol => currentCenterSymbol;

    // Total height of the 5-symbol strip
    private float totalLoopHeight;
    private bool stopRequested = false;

    private void Awake()
    {
        totalLoopHeight = symbolImages.Length * symbolSpacing;
    }

    // Sets up the reel with starting symbols and positions
    public void InitializeReel(SymbolData[] availableSymbols)
    {
        if (symbolImages == null || symbolImages.Length == 0 || availableSymbols == null || availableSymbols.Length == 0)
        {
            Debug.LogError($"[ReelController] Missing symbols or image references on {gameObject.name}!");
            return;
        }

        totalLoopHeight = symbolImages.Length * symbolSpacing;

        // Position the 5 images vertically around Y = 0 (Index 2 is the center payline)
        float startY = -((symbolImages.Length - 1) / 2f) * symbolSpacing;

        for (int i = 0; i < symbolImages.Length; i++)
        {
            RectTransform rect = symbolImages[i].rectTransform;
            float yPos = startY + (i * symbolSpacing);
            rect.anchoredPosition = new Vector2(0f, yPos);

            // Assign a random starting symbol
            int randomIndex = Random.Range(0, availableSymbols.Length);
            symbolImages[i].sprite = availableSymbols[randomIndex].symbolSprite;

            // Keep track of the center symbol (Index 2)
            if (i == 2)
            {
                currentCenterSymbol = availableSymbols[randomIndex];
            }
        }
    }

    // Starts the reel spinning coroutine
    public void StartSpin(SymbolData[] availableSymbols)
    {
        if (isSpinning) return;

        stopRequested = false;
        StartCoroutine(SpinRoutine(availableSymbols));
    }

    // Signals the reel to stop on the target symbol
    public void StopSpin(SymbolData target)
    {
        targetCenterSymbol = target;
        stopRequested = true;
    }

    // Main spinning coroutine: spins, slows down, glides into place, and bounces
    private IEnumerator SpinRoutine(SymbolData[] availableSymbols)
    {
        isSpinning = true;
        float currentSpeed = 0f;
        float accelerationRate = maxSpinSpeed * 4f;

        // 1. Accelerate to full speed and spin until stop is requested
        while (!stopRequested)
        {
            if (currentSpeed < maxSpinSpeed)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpinSpeed, accelerationRate * Time.deltaTime);
            }

            MoveSymbolsDown(currentSpeed * Time.deltaTime, availableSymbols);
            yield return null;
        }

        // 2. Slow down smoothly
        float decelerationTime = 0.35f;
        float elapsed = 0f;
        float speedAtStopRequest = currentSpeed;

        while (elapsed < decelerationTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / decelerationTime;
            currentSpeed = Mathf.Lerp(speedAtStopRequest, 250f, Mathf.SmoothStep(0f, 1f, t));

            MoveSymbolsDown(currentSpeed * Time.deltaTime, availableSymbols);
            yield return null;
        }

        // 3. Put the target symbol on the center row and glide into place
        float startBaseY = -((symbolImages.Length - 1) / 2f) * symbolSpacing;
        float currentCenterY = symbolImages[2].rectTransform.anchoredPosition.y;

        // Set the center symbol (Index 2) to the target symbol
        currentCenterSymbol = targetCenterSymbol;
        symbolImages[2].sprite = targetCenterSymbol.symbolSprite;

        // Give adjacent slots random symbols
        for (int i = 0; i < symbolImages.Length; i++)
        {
            if (i != 2)
            {
                int randIndex = Random.Range(0, availableSymbols.Length);
                symbolImages[i].sprite = availableSymbols[randIndex].symbolSprite;
            }
        }

        // Glide the remaining distance into the grid
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

        // Snap to exact positions
        for (int i = 0; i < symbolImages.Length; i++)
        {
            float targetGridY = startBaseY + (i * symbolSpacing);
            symbolImages[i].rectTransform.anchoredPosition = new Vector2(0f, targetGridY);
        }

        // Play reel stop sound when landing on the target
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayReelStop();
        }

        // 4. Little landing bounce
        yield return StartCoroutine(BounceRoutine(startBaseY));

        isSpinning = false;
    }

    // Moves all symbols down and wraps symbols that go below the bottom back to the top
    private void MoveSymbolsDown(float deltaY, SymbolData[] availableSymbols)
    {
        float bottomLimit = -((symbolImages.Length / 2f) * symbolSpacing) - (symbolSpacing * 0.5f);

        for (int i = 0; i < symbolImages.Length; i++)
        {
            RectTransform rect = symbolImages[i].rectTransform;
            Vector2 pos = rect.anchoredPosition;
            pos.y -= deltaY;

            // When a symbol leaves the bottom, wrap it to the top with a new random sprite
            if (pos.y < bottomLimit)
            {
                pos.y += totalLoopHeight;
                int randIndex = Random.Range(0, availableSymbols.Length);
                symbolImages[i].sprite = availableSymbols[randIndex].symbolSprite;
            }

            rect.anchoredPosition = pos;
        }
    }

    // Small bounce animation when the reel lands
    private IEnumerator BounceRoutine(float baseStartY)
    {
        float halfBounce = bounceDuration * 0.5f;
        float timer = 0f;

        // Move down slightly
        while (timer < halfBounce)
        {
            timer += Time.deltaTime;
            float progress = timer / halfBounce;
            float currentOffset = Mathf.Lerp(0f, -bounceAmount, progress);

            ApplyVerticalOffset(baseStartY, currentOffset);
            yield return null;
        }

        timer = 0f;
        // Spring back up to center
        while (timer < halfBounce)
        {
            timer += Time.deltaTime;
            float progress = timer / halfBounce;
            float currentOffset = Mathf.Lerp(-bounceAmount, 0f, progress);

            ApplyVerticalOffset(baseStartY, currentOffset);
            yield return null;
        }

        // Final centered position
        ApplyVerticalOffset(baseStartY, 0f);
    }

    // Helper to position all symbol slots with an offset
    private void ApplyVerticalOffset(float baseStartY, float offset)
    {
        for (int i = 0; i < symbolImages.Length; i++)
        {
            float targetY = baseStartY + (i * symbolSpacing) + offset;
            symbolImages[i].rectTransform.anchoredPosition = new Vector2(0f, targetY);
        }
    }
}
