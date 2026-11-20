using UnityEngine;

/// <summary>
/// Data structure containing the result of a win evaluation.
/// Contains whether the spin resulted in a win, which symbol matched,
/// the total payout amount, and a descriptive message for the UI.
/// </summary>
[System.Serializable]
public struct WinResult
{
    public bool isWin;
    public SymbolData winningSymbol;
    public int payoutAmount;
    public string resultMessage;
}

/// <summary>
/// WinEvaluator checks whether all reels have matching symbols on the payline.
/// Rule: Player wins when all 3 visible reels land on the exact same symbol.
/// Payout = Current Bet * Winning Symbol's Payout Multiplier.
/// </summary>
public class WinEvaluator : MonoBehaviour
{
    /// <summary>
    /// Checks the center symbols of the 3 reels to determine win or loss.
    /// </summary>
    /// <param name="symbol0">Symbol on Reel 0 center payline</param>
    /// <param name="symbol1">Symbol on Reel 1 center payline</param>
    /// <param name="symbol2">Symbol on Reel 2 center payline</param>
    /// <param name="betAmount">The bet amount placed for this spin</param>
    /// <returns>WinResult containing win status, winning symbol, and payout</returns>
    public WinResult Evaluate(SymbolData symbol0, SymbolData symbol1, SymbolData symbol2, int betAmount)
    {
        WinResult result = new WinResult();

        if (symbol0 == null || symbol1 == null || symbol2 == null)
        {
            Debug.LogError("[WinEvaluator] Received null symbol for evaluation!");
            result.isWin = false;
            result.payoutAmount = 0;
            result.resultMessage = "Error evaluating spin.";
            return result;
        }

        // Check if all 3 reels show the same symbol
        bool allThreeMatch = (symbol0.symbolName == symbol1.symbolName) && 
                             (symbol1.symbolName == symbol2.symbolName);

        if (allThreeMatch)
        {
            // WIN: All 3 symbols match
            result.isWin = true;
            result.winningSymbol = symbol0;
            result.payoutAmount = betAmount * symbol0.payoutMultiplier;
            result.resultMessage = $"JACKPOT! 3x {symbol0.symbolName}! Won {result.payoutAmount}!";
        }
        else
        {
            // LOSS: Symbols do not all match
            result.isWin = false;
            result.winningSymbol = null;
            result.payoutAmount = 0;
            result.resultMessage = "No win. Try again!";
        }

        return result;
    }
}
