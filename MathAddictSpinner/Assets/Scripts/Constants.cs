/*
 * Holds up the constants used throughout the entire game on all facets for easy access.
 * Managed through distinct domain-specific classes.
 */

using System.Collections.Generic;

public static class SpinnerConstants
{
    #region Reels
    // total number of combinations: 15^4
    public const int numberOfReels = 4;
    public const int numberOfSymbols = 8;
    public const int reelLength = 15;

    public static List<float> GetReelSpinsDivisors(float spinDuration)
    {
        return new List<float>
        {
            spinDuration / 100,
            spinDuration / 60,
            spinDuration / 50,
            spinDuration / 30
        }; 
    }

    public static List<float> GetReelSpinsLimits(float spinDuration)
    {
        return new List<float>
        {
            spinDuration / 3,
            spinDuration / 2,
            spinDuration * 2 / 3,
            spinDuration
        };
    }
    
    public static readonly Dictionary<int, string> symbolsMap = new()
    {
        {25, "$"},
        {18, "%"},
        {15, "x"},
        {12, "="},
        {10, ">"},
        {8,  "<"},
        {5,  "+"},
        {3,  "-"}
    };

    // the contents of a reel are a permutation of this list
    public static readonly List<int> reelSetup = new() { 25, 18, 15, 12, 12, 10, 10, 8, 8, 5, 5, 5, 3, 3, 3};
    #endregion
    
    #region Spin
    public const int minIndexDelta = 80;
    public const int maxIndexDelta = 200;
    public const float invalidWager = -1f;
    #endregion
}


public static class GameConstants
{
    public const bool isDebugMode = true;
}

public static class RewardConstants
{
    // jackpot
    public const float startingJackpot = 1000f;
    public const float jackpotWagerMultLB = 5f;
    public const float jackpotWagerMultUB = 12f;
    public const int jackpotSymbol = 15;  // 'inf'
    
    // regular
    public const float multiSymbolMultiplier = 2f;
    public const float midComboSymbolMultiplier = 1.5f;
    public const float smallComboSymbolMultiplier = 1.25f;
    public const float combo2Xwin = 10f;
    public const float combo3x2xwin = 25f;
}


public static class UIConstants
{
    #region Misc Text
    public const string onHoldText = "Spin\n2\nwin!";
    public const string noWagerReceivedText = "Try\nAgain!";
    public const string successText = "You've\nWON!";
    public const string jackpotText = "JACKPOT!";
    public const string lossText = "Try\nAgain!";

    public const string wagerText = "Wager:\n $";
    public const string lastWinText = "Last Win:\n $";
    #endregion

}



