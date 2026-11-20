using UnityEngine;

/// <summary>
/// GameManager controls overall game initialization, state, and session management.
/// It coordinates between the SlotMachineController and high-level game session flow.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Controllers")]
    [Tooltip("Reference to the main slot machine controller")]
    [SerializeField] private SlotMachineController slotMachineController;

    [Tooltip("Reference to the wallet manager")]
    [SerializeField] private WalletManager walletManager;

    private void Awake()
    {
        // Simple Singleton pattern for easy access
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Resets the game balance to starting funds.
    /// Useful when the player runs out of credits.
    /// </summary>
    public void RestartGame()
    {
        if (walletManager != null)
        {
            walletManager.ResetBalance();
        }
    }
}
