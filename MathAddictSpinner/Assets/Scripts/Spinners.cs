using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * Holds up the math and general API for computations for RTP when spinning
 */
public class Spinners : MonoBehaviour
{
    // each reel is made up of a permutation of all symbols that are available (len 8)
    private List<int> reel1 = new List<int>();
    private List<int> reel2 = new List<int>();
    private List<int> reel3 = new List<int>();
    private List<int> reel4 = new List<int>();
    
    // indexes indicate where on the reel we are currently on (starts at 1 to have the i-1 for the row above)
    private int reel1Index = 1;
    private int reel2Index = 1;
    private int reel3Index = 1;
    private int reel4Index = 1;
    
    // reward system
    private float lastReward = -1f;
    private float currentReward = -1f;
    private float currentJackpot = -GameConstants.startingJackpot;
    private bool jackpotTriggered = false;

    // debug
    private int[][] current3By4 =
    {
        new int[] {0, 0, 0, 0 },
        new int[] {0, 0, 0, 0 },
        new int[] {0, 0, 0, 0 },
        new int[] {0, 0, 0, 0 },
    };

    private void Start()
    {
        SetupSessionReels();
    }
    
    // Spin Functions
    public SpinResult TriggerSpin(float wager)
    {
        // compute a number of positions for each reel to move their index
        reel1Index = (reel1Index + Random.Range(SpinnerConstants.minIndexDelta, SpinnerConstants.maxIndexDelta)) % SpinnerConstants.reelLength;
        reel2Index = (reel2Index + Random.Range(SpinnerConstants.minIndexDelta, SpinnerConstants.maxIndexDelta)) % SpinnerConstants.reelLength;
        reel3Index = (reel3Index + Random.Range(SpinnerConstants.minIndexDelta, SpinnerConstants.maxIndexDelta)) % SpinnerConstants.reelLength;
        reel4Index = (reel4Index + Random.Range(SpinnerConstants.minIndexDelta, SpinnerConstants.maxIndexDelta)) % SpinnerConstants.reelLength;
        
        SpinResult spinResult = new SpinResult(GetRtp(wager), reel1Index, reel2Index, reel3Index, reel4Index, jackpotTriggered);
        PrintMatrix(current3By4);
        return spinResult;
    }   
    
    private float GetRtp(float wager)
    {
        // compute RTP based on the current indexes of each reel, which determines the position of the 3 rows that get
        // shown, checking for special combinations, and overall value of the 4x3 rectangle
        
        // setup state
        lastReward = currentReward;
        currentReward = 0f;
        jackpotTriggered = false;
        
        // collect the values present in the 4x3
        Dictionary<int, int> currentSpinCounts = GetCurrent3By4Counts();
        
        // check for counts
        foreach (KeyValuePair<int, int> symbolCount in currentSpinCounts)
        {
            // if >= 3 in screen, add # of symbols * symbol value to total result
            currentReward += symbolCount.Value >= 3 ? (symbolCount.Value * symbolCount.Key) : 0;
        }
        
        // check for 4 in middle row
        int toMatch = reel1[reel1Index];
        if (toMatch == reel2[reel2Index] && toMatch == reel3[reel3Index] && toMatch == reel4[reel4Index])
        {
            currentReward += (float)Math.Pow(toMatch, 3);

            if (toMatch == GameConstants.jackpotSymbol)
            {
                currentReward += currentJackpot;
                jackpotTriggered = true;
                currentJackpot = GameConstants.startingJackpot;  // reset
            }
            else
            {
                // if no jackpot, add % of wager into Jackpot
                currentJackpot += Random.Range(GameConstants.jackpotWagerMultLB, GameConstants.jackpotWagerMultUB) * wager;
            }
        }

        currentReward *= wager / 2;
        return currentReward;
    }

    private Dictionary<int, int> GetCurrent3By4Counts()
    {
        // setup as 0s for everything
        Dictionary<int, int> currentSpinCounts = new Dictionary<int, int>();
        foreach (int key in SpinnerConstants.symbolsMap.Keys)
        {
            currentSpinCounts[key] = 0;
        }

        // get the counts
        CountSymbolsInReel(reel1, reel1Index, currentSpinCounts, 1);
        CountSymbolsInReel(reel2, reel2Index, currentSpinCounts, 2);
        CountSymbolsInReel(reel3, reel3Index, currentSpinCounts, 3);
        CountSymbolsInReel(reel4, reel4Index, currentSpinCounts, 4);
        
        return currentSpinCounts;
    }

    // precondition is that the input dictionary does contain as keys all possible values
    private void CountSymbolsInReel(List<int> reel, int reelIndex, Dictionary<int, int> counts, int reelNum)
    {
        int len = SpinnerConstants.reelLength;
        int key = 0;
        // C# passes the ref to the actual results dictionary, so we can modify it inside w/o need to return
        for (int i = -1; i <= 1; i++)
        {
            key = reel[(reelIndex + i + len) % len];
            counts[key] += 1;
            current3By4[i+1][reelNum] = key;
        }
    }

    #region Setup
    private void SetupSessionReels()
    {
        List<int> elements = SpinnerConstants.reelSetup;
        reel1 = ShuffleReel(elements);
        reel2 = ShuffleReel(reel1);
        reel3 = ShuffleReel(reel2);
        reel4 = ShuffleReel(reel3);
    }

    // fisher-yates shuffle
    private List<int> ShuffleReel(List<int> elements)
    {
        List<int> result = new List<int>(elements);
        
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }

        return result;
    }
    #endregion
    
    #region SpinResult
    public struct SpinResult
    {
        public float rtp;
        public int reel1Index;
        public int reel2Index;
        public int reel3Index;
        public int reel4Index;
        public bool jackpotTriggered;

        public SpinResult(float win, int index1, int index2, int index3, int index4, bool jackpot)
        {
            rtp = win;
            reel1Index = index1;
            reel2Index = index2;
            reel3Index = index3;
            reel4Index = index4;
            jackpotTriggered = jackpot;
        }
    }
    #endregion
    
    #region Debug
    
    private void PrintMatrix(int[][] mat)
    {
        Debug.Log("Printing current spin results (3x4):");
        foreach (var row in mat)
        {
            Debug.Log(string.Join(" ", row));
        }
        Debug.Log("\n#--------#\n");
    }
    #endregion
}
