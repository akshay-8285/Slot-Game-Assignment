using System;
using UnityEngine;

/// <summary>
/// WalletManager tracks the player's balance and current bet amount.
/// It provides clean methods to check if the player can afford a spin,
/// deduct the bet when spinning, and add payouts upon winning.
/// </summary>
public class WalletManager : MonoBehaviour
{
    [Header("Wallet Settings")]
    [Tooltip("Starting credits given to the player when the game starts")]
    [SerializeField] private int startingBalance = 1000;

    [Tooltip("Default bet amount per spin")]
    [SerializeField] private int defaultBet = 10;

    // Runtime state
    private int currentBalance;
    private int currentBet;

    // Public read-only accessors
    public int CurrentBalance => currentBalance;
    public int CurrentBet => currentBet;

    // Events triggered when values change, allowing UI to update automatically
    public event Action<int> OnBalanceChanged;
    public event Action<int> OnBetChanged;

    private void Awake()
    {
        currentBalance = startingBalance;
        currentBet = defaultBet;
    }

    private void Start()
    {
        // Notify listeners of initial values
        OnBalanceChanged?.Invoke(currentBalance);
        OnBetChanged?.Invoke(currentBet);
    }

    /// <summary>
    /// Checks whether the player has enough balance to place the current bet.
    /// </summary>
    public bool CanAffordBet()
    {
        return currentBalance >= currentBet;
    }

    /// <summary>
    /// Deducts the current bet amount from balance.
    /// Call this when a spin begins.
    /// </summary>
    public bool TryDeductBet()
    {
        if (CanAffordBet())
        {
            currentBalance -= currentBet;
            OnBalanceChanged?.Invoke(currentBalance);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Adds winnings to the player's balance.
    /// </summary>
    public void AddPayout(int amount)
    {
        if (amount > 0)
        {
            currentBalance += amount;
            OnBalanceChanged?.Invoke(currentBalance);
        }
    }

    /// <summary>
    /// Changes the current bet amount.
    /// </summary>
    public void SetBet(int newBet)
    {
        if (newBet > 0)
        {
            currentBet = newBet;
            OnBetChanged?.Invoke(currentBet);
        }
    }

    /// <summary>
    /// Resets the wallet balance back to starting balance.
    /// </summary>
    public void ResetBalance()
    {
        currentBalance = startingBalance;
        OnBalanceChanged?.Invoke(currentBalance);
    }
}
