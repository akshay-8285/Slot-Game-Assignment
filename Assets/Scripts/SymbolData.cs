using UnityEngine;

/// <summary>
/// SymbolData holds all information about a single slot symbol.
/// It defines what the symbol is called, how it looks (Sprite),
/// and how much it pays when a winning combination is hit.
/// </summary>
[System.Serializable]
public class SymbolData
{
    [Header("Symbol Identity")]
    [Tooltip("Unique name of this symbol (e.g., 'Seven', 'Cherry', 'Bell', 'BAR')")]
    public string symbolName;

    [Tooltip("The 2D sprite image displayed on the reel for this symbol")]
    public Sprite symbolSprite;

    [Header("Payout Configuration")]
    [Tooltip("Payout multiplier for this symbol. Payout = Current Bet * Payout Multiplier")]
    public int payoutMultiplier = 10;
}
