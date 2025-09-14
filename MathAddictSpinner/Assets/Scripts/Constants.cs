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
    #endregion
}


public static class GameConstants
{
    public const float startingJackpot = 1000f;
    public const float jackpotWagerMultLB = 5f;
    public const float jackpotWagerMultUB = 12f;
    public const int jackpotSymbol = 15;  // 'x'
    public const bool isDebugMode = true;
}


public static class UIConstants
{
    #region Result Text
    public const string onHoldText = "Spin to win!";
    public const string successText = "You have WON!";
    public const string jackpotText = "JACKPOT! BIG WIN!";
    public const string lossText = "LOST ~~ Try Again!";
    #endregion
    
    #region UI Result
    #endregion
}



