using System;
using UnityEngine;

// Tracks the player's balance and current bet amount.
public class WalletManager : MonoBehaviour
{
    [Header("Wallet Settings")]
    [SerializeField] private int startingBalance = 1000;
    [SerializeField] private int defaultBet = 10;

    private int currentBalance;
    private int currentBet;

    public int CurrentBalance => currentBalance;
    public int CurrentBet => currentBet;

    // Events to notify the UI when balance or bet changes
    public event Action<int> OnBalanceChanged;
    public event Action<int> OnBetChanged;

    private void Awake()
    {
        currentBalance = startingBalance;
        currentBet = defaultBet;
    }

    private void Start()
    {
        // Update UI with initial values
        OnBalanceChanged?.Invoke(currentBalance);
        OnBetChanged?.Invoke(currentBet);
    }

    // Checks if the player has enough credits to place the bet
    public bool CanAffordBet()
    {
        return currentBalance >= currentBet;
    }

    // Deducts the bet when a spin starts
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

    // Adds winnings to the balance
    public void AddPayout(int amount)
    {
        if (amount > 0)
        {
            currentBalance += amount;
            OnBalanceChanged?.Invoke(currentBalance);
        }
    }

    // Changes the current bet amount
    public void SetBet(int newBet)
    {
        if (newBet > 0)
        {
            currentBet = newBet;
            OnBetChanged?.Invoke(currentBet);
        }
    }

    // Resets the balance back to starting credits
    public void ResetBalance()
    {
        currentBalance = startingBalance;
        OnBalanceChanged?.Invoke(currentBalance);
    }
}
