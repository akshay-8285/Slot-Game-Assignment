using UnityEngine;

// Stores the outcome of a spin
[System.Serializable]
public struct WinResult
{
    public bool isWin;
    public SymbolData winningSymbol;
    public int payoutAmount;
    public string resultMessage;
}

// Checks if the 3 center symbols match and calculates winnings.
public class WinEvaluator : MonoBehaviour
{
    // Evaluates center symbols (player wins when all 3 match)
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

        // Check if all 3 center symbols are the same
        bool allThreeMatch = (symbol0.symbolName == symbol1.symbolName) && 
                             (symbol1.symbolName == symbol2.symbolName);

        if (allThreeMatch)
        {
            // Win: payout = bet * multiplier
            result.isWin = true;
            result.winningSymbol = symbol0;
            result.payoutAmount = betAmount * symbol0.payoutMultiplier;
            result.resultMessage = $"JACKPOT! 3x {symbol0.symbolName}! Won {result.payoutAmount}!";
        }
        else
        {
            // Loss: no match
            result.isWin = false;
            result.winningSymbol = null;
            result.payoutAmount = 0;
            result.resultMessage = "No win. Try again!";
        }

        return result;
    }
}
