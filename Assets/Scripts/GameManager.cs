using UnityEngine;

// Coordinates the game session and allows restarting the game.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Controllers")]
    [SerializeField] private SlotMachineController slotMachineController;
    [SerializeField] private WalletManager walletManager;

    private void Awake()
    {
        // Singleton setup so other scripts can access GameManager easily
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Resets the player's balance back to the starting amount
    public void RestartGame()
    {
        if (walletManager != null)
        {
            walletManager.ResetBalance();
        }
    }
}
