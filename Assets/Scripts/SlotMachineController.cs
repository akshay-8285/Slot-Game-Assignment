using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SlotMachineController coordinates the entire slot machine gameplay sequence.
/// Responsibilities:
/// 1. Responds to the player pressing the Spin button.
/// 2. Validates player bet and prevents multiple spins at once.
/// 3. Animates the side pull-lever.
/// 4. Generates random outcomes using Unity's Random Number Generator (RNG).
/// 5. Starts all 3 reels spinning simultaneously.
/// 6. Sequentially stops each reel with realistic delays.
/// 7. Checks for winning combinations and triggers payouts.
/// </summary>
public class SlotMachineController : MonoBehaviour
{
    [Header("Core Components")]
    [Tooltip("The 3 reel controllers (Reel 0 = Left, Reel 1 = Center, Reel 2 = Right)")]
    [SerializeField] private ReelController[] reels;

    [Tooltip("List of all symbols in the game with their sprites and payout multipliers")]
    [SerializeField] private SymbolData[] availableSymbols;

    [Tooltip("Evaluates whether stopped reels form a winning combination")]
    [SerializeField] private WinEvaluator winEvaluator;

    [Tooltip("Manages balance and bet amount")]
    [SerializeField] private WalletManager walletManager;

    [Header("Spin Timing")]
    [Tooltip("Minimum time in seconds that reels spin before Reel 0 begins stopping")]
    [SerializeField] private float baseSpinDuration = 1.0f;

    [Tooltip("Delay in seconds between Reel 0 stopping, Reel 1 stopping, and Reel 2 stopping")]
    [SerializeField] private float sequentialStopDelay = 0.35f;

    [Header("UI References")]
    [Tooltip("The main SPIN button")]
    [SerializeField] private Button spinButton;

    [Tooltip("Text displaying game status ('Press Spin', 'JACKPOT!', etc.)")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Tooltip("Text displaying the last win payout amount")]
    [SerializeField] private TextMeshProUGUI winAmountText;

    [Tooltip("Text displaying the current balance")]
    [SerializeField] private TextMeshProUGUI balanceText;

    [Tooltip("Text displaying the current bet amount")]
    [SerializeField] private TextMeshProUGUI betText;

    [Header("Lever Animation")]
    [Tooltip("UI Image component for the pull lever on the right")]
    [SerializeField] private Image leverImage;

    [Tooltip("Sprite for the lever in the upright / idle position")]
    [SerializeField] private Sprite leverUpSprite;

    [Tooltip("Sprite for the lever in the pulled-down position")]
    [SerializeField] private Sprite leverDownSprite;

    [Header("Win Popup (Optional)")]
    [Tooltip("Optional popup dialog shown on a big win")]
    [SerializeField] private GameObject winPopupObject;

    [Tooltip("Text inside the win popup")]
    [SerializeField] private TextMeshProUGUI winPopupText;

    [Header("Editor Debug (Testing Only)")]
    [Tooltip("Enable to force a win with the chosen symbol in Editor (for testing)")]
    [SerializeField] private bool debugForceWin = false;

    [Tooltip("Symbol index to force for testing (0 = Seven, 1 = Cherry, 2 = Bell, 3 = BAR)")]
    [SerializeField] private int debugForceSymbolIndex = 0;

    // Runtime state
    private bool isSpinInProgress = false;
    public bool IsSpinInProgress => isSpinInProgress;

    private void Start()
    {
        // Initialize all reels with starting symbols
        if (reels != null)
        {
            for (int i = 0; i < reels.Length; i++)
            {
                if (reels[i] != null)
                {
                    reels[i].InitializeReel(availableSymbols);
                }
            }
        }

        // Hook up wallet change events to keep UI updated
        if (walletManager != null)
        {
            walletManager.OnBalanceChanged += UpdateBalanceUI;
            walletManager.OnBetChanged += UpdateBetUI;
            UpdateBalanceUI(walletManager.CurrentBalance);
            UpdateBetUI(walletManager.CurrentBet);
        }

        // Set initial status text
        if (statusText != null)
        {
            statusText.text = "PRESS SPIN TO PLAY";
        }

        if (winAmountText != null)
        {
            winAmountText.text = "WIN: 0";
        }

        // Ensure lever starts in upright position
        if (leverImage != null && leverUpSprite != null)
        {
            leverImage.sprite = leverUpSprite;
        }

        // Hide win popup at start
        if (winPopupObject != null)
        {
            winPopupObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (walletManager != null)
        {
            walletManager.OnBalanceChanged -= UpdateBalanceUI;
            walletManager.OnBetChanged -= UpdateBetUI;
        }
    }

    /// <summary>
    /// Called when the player clicks the Spin Button or pulls the lever.
    /// </summary>
    public void OnSpinButtonPressed()
    {
        // Guard clause: prevent multiple spins at the same time
        if (isSpinInProgress) return;

        // Check wallet balance
        if (walletManager != null && !walletManager.CanAffordBet())
        {
            if (statusText != null)
            {
                statusText.text = "NOT ENOUGH CREDITS!";
            }
            return;
        }

        StartCoroutine(SpinSequenceRoutine());
    }

    /// <summary>
    /// Master coroutine executing the complete spin workflow.
    /// </summary>
    private IEnumerator SpinSequenceRoutine()
    {
        isSpinInProgress = true;

        // 1. Disable the Spin button so player cannot click it during spin
        if (spinButton != null)
        {
            spinButton.interactable = false;
        }

        // 2. Hide win popup if previously open
        if (winPopupObject != null)
        {
            winPopupObject.SetActive(false);
        }

        // 3. Deduct bet from wallet
        int currentBet = 10;
        if (walletManager != null)
        {
            currentBet = walletManager.CurrentBet;
            walletManager.TryDeductBet();
        }

        if (statusText != null)
        {
            statusText.text = "SPINNING...";
        }

        // 4. Animate the lever pull (lever down, wait, lever up)
        yield return StartCoroutine(AnimateLeverRoutine());

        // 5. Generate target outcome using Unity RNG
        // The outcome is calculated BEFORE animations begin, ensuring fairness and reproducibility
        SymbolData[] targetSymbols = new SymbolData[reels.Length];

#if UNITY_EDITOR
        if (debugForceWin && availableSymbols != null && availableSymbols.Length > 0)
        {
            int safeIndex = Mathf.Clamp(debugForceSymbolIndex, 0, availableSymbols.Length - 1);
            for (int i = 0; i < reels.Length; i++)
            {
                targetSymbols[i] = availableSymbols[safeIndex];
            }
        }
        else
#endif
        {
            for (int i = 0; i < reels.Length; i++)
            {
                int randomIndex = Random.Range(0, availableSymbols.Length);
                targetSymbols[i] = availableSymbols[randomIndex];
            }
        }

        // 6. Start all 3 reels spinning simultaneously
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].StartSpin(availableSymbols);
        }

        // 7. Wait for base spin duration
        yield return new WaitForSeconds(baseSpinDuration);

        // 8. Sequentially stop each reel one after another
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].StopSpin(targetSymbols[i]);

            // Add delay before stopping the next reel
            if (i < reels.Length - 1)
            {
                yield return new WaitForSeconds(sequentialStopDelay);
            }
        }

        // 9. Wait until the final reel has finished its bounce animation
        while (reels[reels.Length - 1].IsSpinning)
        {
            yield return null;
        }

        // 10. Evaluate win condition
        // Rule: Player wins when all visible center reel symbols match
        if (winEvaluator != null)
        {
            WinResult result = winEvaluator.Evaluate(
                reels[0].CurrentCenterSymbol,
                reels[1].CurrentCenterSymbol,
                reels[2].CurrentCenterSymbol,
                currentBet
            );

            if (result.isWin)
            {
                // Player won!
                if (walletManager != null)
                {
                    walletManager.AddPayout(result.payoutAmount);
                }

                if (statusText != null)
                {
                    statusText.text = result.resultMessage;
                }

                if (winAmountText != null)
                {
                    winAmountText.text = $"WIN: +{result.payoutAmount}";
                }

                // Show win popup for significant wins
                if (winPopupObject != null && winPopupText != null)
                {
                    winPopupText.text = $"CONGRATULATIONS!\n\nYOU WON {result.payoutAmount} CREDITS!";
                    winPopupObject.SetActive(true);
                }
            }
            else
            {
                // Player lost
                if (statusText != null)
                {
                    statusText.text = result.resultMessage;
                }

                if (winAmountText != null)
                {
                    winAmountText.text = "WIN: 0";
                }
            }
        }

        // 11. Re-enable Spin button for the next round
        if (spinButton != null)
        {
            spinButton.interactable = true;
        }

        isSpinInProgress = false;
    }

    /// <summary>
    /// Animates the side pull lever from upright to pulled down and back.
    /// </summary>
    private IEnumerator AnimateLeverRoutine()
    {
        if (leverImage != null && leverDownSprite != null)
        {
            leverImage.sprite = leverDownSprite;
            yield return new WaitForSeconds(0.18f);

            if (leverUpSprite != null)
            {
                leverImage.sprite = leverUpSprite;
            }
        }
    }

    /// <summary>
    /// Updates the balance text in the UI.
    /// </summary>
    private void UpdateBalanceUI(int newBalance)
    {
        if (balanceText != null)
        {
            balanceText.text = $"BALANCE: {newBalance}";
        }
    }

    /// <summary>
    /// Updates the bet text in the UI.
    /// </summary>
    private void UpdateBetUI(int newBet)
    {
        if (betText != null)
        {
            betText.text = $"BET: {newBet}";
        }
    }

    /// <summary>
    /// Bet selector helper method called by UI Bet buttons (e.g., 10, 50, 100).
    /// </summary>
    public void SelectBet(int amount)
    {
        if (isSpinInProgress) return;

        if (walletManager != null)
        {
            walletManager.SetBet(amount);
        }
    }

    /// <summary>
    /// Closes the win popup modal dialog.
    /// </summary>
    public void CloseWinPopup()
    {
        if (winPopupObject != null)
        {
            winPopupObject.SetActive(false);
        }
    }
}
