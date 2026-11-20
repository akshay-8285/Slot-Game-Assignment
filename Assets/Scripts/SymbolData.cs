using UnityEngine;

// Holds data for one slot symbol.
[System.Serializable]
public class SymbolData
{
    [Header("Symbol Identity")]
    public string symbolName;
    public Sprite symbolSprite;

    [Header("Payout Configuration")]
    // Payout multiplier (Payout = Bet * multiplier)
    public int payoutMultiplier = 10;
}

