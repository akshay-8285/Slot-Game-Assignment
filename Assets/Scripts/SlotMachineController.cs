using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Main controller: coordinates the spin sequence, RNG, lever animation, and win evaluation.
public class SlotMachineController : MonoBehaviour
{
    [Header("Core Components")]
    [SerializeField] private ReelController[] reels;
    [SerializeField] private SymbolData[] availableSymbols;
    [SerializeField] private WinEvaluator winEvaluator;
    [SerializeField] private WalletManager walletManager;

    [Header("Spin Timing")]
    [SerializeField] private float baseSpinDuration = 1.0f;
    [SerializeField] private float sequentialStopDelay = 0.35f;

    [Header("UI References")]
    [SerializeField] private Button spinButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI winAmountText;
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private TextMeshProUGUI betText;

    [Header("Lever Animation")]
    [SerializeField] private Image leverImage;
    [SerializeField] private Sprite leverUpSprite;
    [SerializeField] private Sprite leverDownSprite;

    [Header("Win Popup")]
    [SerializeField] private GameObject winPopupObject;
    [SerializeField] private TextMeshProUGUI winPopupText;

    [Header("Editor Debug (Testing Only)")]
    [SerializeField] private bool debugForceWin = false;
    [SerializeField] private int debugForceSymbolIndex = 0;

    private bool isSpinInProgress = false;
    public bool IsSpinInProgress => isSpinInProgress;

    private void Start()
    {
        // Setup starting symbols on all 3 reels
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

        // Listen to wallet events to keep the UI updated
        if (walletManager != null)
        {
            walletManager.OnBalanceChanged += UpdateBalanceUI;
            walletManager.OnBetChanged += UpdateBetUI;
            UpdateBalanceUI(walletManager.CurrentBalance);
            UpdateBetUI(walletManager.CurrentBet);
        }

        // Set default UI texts
        if (statusText != null)
        {
            statusText.text = "PRESS SPIN TO PLAY";
        }

        if (winAmountText != null)
        {
            winAmountText.text = "WIN: 0";
        }

        // Make sure lever starts upright and popup is hidden
        if (leverImage != null && leverUpSprite != null)
        {
            leverImage.sprite = leverUpSprite;
        }

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

    // Called when the player clicks the Spin button or pulls the lever
    public void OnSpinButtonPressed()
    {
        // Don't spin if already spinning
        if (isSpinInProgress) return;

        // Check if player has enough credits
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

    // Master coroutine for the spin sequence
    private IEnumerator SpinSequenceRoutine()
    {
        isSpinInProgress = true;

        // 1. Disable spin button and hide old popup
        if (spinButton != null)
        {
            spinButton.interactable = false;
        }

        if (winPopupObject != null)
        {
            winPopupObject.SetActive(false);
        }

        // 2. Deduct the bet amount
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

        // 3. Play lever pull animation
        yield return StartCoroutine(AnimateLeverRoutine());

        // 4. Pick random target symbols for each reel using Unity RNG
        // Outcome is chosen before movement starts so visual landing matches the result
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

        // 5. Start all reels spinning together
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].StartSpin(availableSymbols);
        }

        // 6. Wait before stopping reels
        yield return new WaitForSeconds(baseSpinDuration);

        // 7. Stop each reel one by one from left to right
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].StopSpin(targetSymbols[i]);

            if (i < reels.Length - 1)
            {
                yield return new WaitForSeconds(sequentialStopDelay);
            }
        }

        // 8. Wait until the last reel finishes bouncing
        while (reels[reels.Length - 1].IsSpinning)
        {
            yield return null;
        }

        // 9. Check if all 3 center symbols match
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
                // Player won: add credits and show win messages
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

                // Show win popup
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

        // 10. Re-enable spin button for next round
        if (spinButton != null)
        {
            spinButton.interactable = true;
        }

        isSpinInProgress = false;
    }

    // Pulls lever down and returns it upright
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

    // Update balance text
    private void UpdateBalanceUI(int newBalance)
    {
        if (balanceText != null)
        {
            balanceText.text = $"BALANCE: {newBalance}";
        }
    }

    // Update bet text
    private void UpdateBetUI(int newBet)
    {
        if (betText != null)
        {
            betText.text = $"BET: {newBet}";
        }
    }

    // Called by UI bet buttons (10, 50, 100)
    public void SelectBet(int amount)
    {
        if (isSpinInProgress) return;

        if (walletManager != null)
        {
            walletManager.SetBet(amount);
        }
    }

    // Closes the win popup modal
    public void CloseWinPopup()
    {
        if (winPopupObject != null)
        {
            winPopupObject.SetActive(false);
        }
    }
}
