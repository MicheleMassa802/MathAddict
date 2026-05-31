using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * Holds up the math and general API for computations for RTP when spinning
 */
public class Spinners : MonoBehaviour
{
    // indexes indicate where on the reel we are currently on (starts at 1 to have the i-1 for the row above)
    private int reel1Index = 1;
    private int reel2Index = 1;
    private int reel3Index = 1;
    private int reel4Index = 1;
    
    // reward system
    private float lastReward = -1f;
    private float currentReward = -1f;
    private float currentJackpot = -RewardConstants.startingJackpot;
    private bool jackpotTriggered = false;
    
    /*
     * At the resolution of each spinner i, we have the list of 3 ints indicating:
     *  - At index 0: # of inf (jackpot) symbols on the middle row
     *  - At index 1: # of doubles on the board so far
     *  - At index 2: # of triples on the board so far
     *
     * Such that when the third spin resolves, we can go into ComboProgressAtReelResolveX[2],
     * and deduce the progress for the combo indicators
     */
    private List<List<int>> comboProgressAtReelResolveX;
    private List<List<int>> keyTargetsThisSpin;
    // debug
    private int[][] current3By4 =
    {
        new int[] {0, 0, 0, 0 },
        new int[] {0, 0, 0, 0 },
        new int[] {0, 0, 0, 0 }
    };
    
    // Spin Functions
    public SpinResult TriggerSpin(float wager)
    {
        // compute a number of positions for each reel to move their index
        reel1Index = (reel1Index + Random.Range(SpinnerConstants.minIndexDelta, SpinnerConstants.maxIndexDelta)) % SpinnerConstants.reelLength;
        reel2Index = (reel2Index + Random.Range(SpinnerConstants.minIndexDelta, SpinnerConstants.maxIndexDelta)) % SpinnerConstants.reelLength;
        reel3Index = (reel3Index + Random.Range(SpinnerConstants.minIndexDelta, SpinnerConstants.maxIndexDelta)) % SpinnerConstants.reelLength;
        reel4Index = (reel4Index + Random.Range(SpinnerConstants.minIndexDelta, SpinnerConstants.maxIndexDelta)) % SpinnerConstants.reelLength;

        float win = GetRtp(wager);
        SpinResult spinResult = new SpinResult(win, reel1Index, reel2Index,
            reel3Index, reel4Index, jackpotTriggered, comboProgressAtReelResolveX, keyTargetsThisSpin);
        
        #if UNITY_EDITOR
            PrintMatrix(current3By4);
        #endif
        
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
        
        int number2X = 0;
        int number3X = 0;
        // check for counts
        foreach (KeyValuePair<int, int> symbolCount in currentSpinCounts)
        {
            if (symbolCount.Value == 2) number2X++;
            if (symbolCount.Value == 3) number3X++;
            if (symbolCount.Value >= 5)
            {
                currentReward += symbolCount.Value * symbolCount.Key * RewardConstants.multiSymbolMultiplier;
            }
        }
        
        // check for 4 2X counts
        if (number2X >= 4)
        {
            currentReward += number2X * RewardConstants.combo2Xwin;
            currentReward *= RewardConstants.smallComboSymbolMultiplier;
        }
        
        // check for 2 3X count and 2 2X
        if (number2X >= 2 && number3X >= 2)
        {
            currentReward += number2X * number3X * RewardConstants.combo3x2xwin;
            currentReward *= RewardConstants.midComboSymbolMultiplier;
        }
        
        // check for 4 in middle row
        int toMatch = MAUnityManager.Instance.reels[0][reel1Index];
        if (toMatch == MAUnityManager.Instance.reels[1][reel2Index] &&
            toMatch == MAUnityManager.Instance.reels[2][reel3Index] &&
            toMatch == MAUnityManager.Instance.reels[3][reel4Index])
        {
            currentReward += (float)Math.Pow(toMatch, 3);

            if (toMatch == RewardConstants.jackpotSymbol)
            {
                currentReward += currentJackpot;
                jackpotTriggered = true;
                currentJackpot = RewardConstants.startingJackpot;  // reset
            }
            else
            {
                // if no jackpot, add % of wager into Jackpot
                currentJackpot += Random.Range(RewardConstants.jackpotWagerMultLB, RewardConstants.jackpotWagerMultUB) * wager;
            }
        }

        currentReward *= wager / 2;

        if (currentReward > 0)
        {
            // fetch positions of the 2 symbols with the highest spin counts
            GetKeyTargetIndices(currentSpinCounts);
        }
        // if no reward, array left empty
        
        return (float)Math.Round(currentReward, 2);
    }

    private List<List<int>> GetCleanComboProgressMatrix()
    {
        return new List<List<int>>
        {
            new() {0, 0, 0},
            new() {0, 0, 0},
            new() {0, 0, 0},
            new() {0, 0, 0}
        };
    }

    // given the spin counts, returns the progress to jackpot, number of doubles and triples
    private List<int> GetComboIndicatorInfo(Dictionary<int, int> currentSpinCounts, int reelsChecked)
    {
        int number2X = 0;
        int number3X = 0;
        foreach (KeyValuePair<int, int> symbolCount in currentSpinCounts)
        {
            if (symbolCount.Value >= 2) number2X++;
            if (symbolCount.Value >= 3) number3X++;
        }
        
        // check for 4 in middle row
        Dictionary<int, int> middleRowCount = new Dictionary<int, int>();
        middleRowCount[MAUnityManager.Instance.reels[0][reel1Index]] = 1;
        for (int r = 1; r < reelsChecked; r++)
        {
            int index = r switch
            {
                1 => reel2Index,
                2 => reel3Index,
                3 => reel4Index,
                _ => 0
            };

            int key = MAUnityManager.Instance.reels[r][index];
            middleRowCount[key] = middleRowCount.TryGetValue(key, out int count)
                ? count + 1
                : 1;
        }

        int maxEquals = 1;
        foreach (KeyValuePair<int, int> numberOfEqualsInMiddleRow in middleRowCount)
        {
            if (numberOfEqualsInMiddleRow.Value > maxEquals)
            {
                maxEquals = numberOfEqualsInMiddleRow.Value;
            }
        }
        return new List<int>{maxEquals, number2X, number3X};
    }
        
    private Dictionary<int, int> GetCurrent3By4Counts()
    {
        // setup as 0s for everything
        Dictionary<int, int> currentSpinCounts = new Dictionary<int, int>();
        foreach (int key in SpinnerConstants.symbolsMapKeys)
        {
            currentSpinCounts[key] = 0;
        }

        comboProgressAtReelResolveX = GetCleanComboProgressMatrix();
        
        // get the counts and check for 
        CountSymbolsInReel(MAUnityManager.Instance.reels[0], reel1Index, currentSpinCounts, 1);
        comboProgressAtReelResolveX[0] = GetComboIndicatorInfo(currentSpinCounts, 1);
        CountSymbolsInReel(MAUnityManager.Instance.reels[1], reel2Index, currentSpinCounts, 2);
        comboProgressAtReelResolveX[1] = GetComboIndicatorInfo(currentSpinCounts, 2);
        CountSymbolsInReel(MAUnityManager.Instance.reels[2], reel3Index, currentSpinCounts, 3);
        comboProgressAtReelResolveX[2] = GetComboIndicatorInfo(currentSpinCounts, 3);
        CountSymbolsInReel(MAUnityManager.Instance.reels[3], reel4Index, currentSpinCounts, 4);
        comboProgressAtReelResolveX[3] = GetComboIndicatorInfo(currentSpinCounts, 4);
        
        return currentSpinCounts;
    }

    // precondition is that the input dictionary does contain as keys all possible values
    private void CountSymbolsInReel(List<int> reel, int reelIndex, Dictionary<int, int> counts, int reelNum)
    {
        int len = SpinnerConstants.reelLength;
        int key;
        // C# passes the ref to the actual results dictionary, so we can modify it inside w/o need to return
        for (int i = -1; i <= 1; i++)
        {
            key = reel[(reelIndex + i + len) % len];
            counts[key] += 1;
            current3By4[i+1][reelNum-1] = key;
        }
    }

    private void GetKeyTargetIndices(Dictionary<int, int> currentSpinCounts)
    {
        keyTargetsThisSpin = new List<List<int>>();

        var top2Symbols = currentSpinCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(2)
            .Select(kvp => kvp.Key)
            .ToList();
        
        // fetch indices of those 2 top symbols
        foreach (var symbol in top2Symbols)
        {
            keyTargetsThisSpin.Add(GetIndicesForSymbolIn3By4(symbol));
        }
    }

    private List<int> GetIndicesForSymbolIn3By4(int symbol)
    {
        List<int> indices = new List<int>();

        for (int r=0; r < current3By4.Length; r++)
        {
            var row = current3By4[r];
            for (int c = 0; c < row.Length; c++)
            {
                if (symbol == row[c])
                {
                    indices.Add(c*3 + r);
                }
            }
        }
        indices.Sort();
        // Debug.Log($"{symbol} can be found at indices: {string.Join(",", indices)}");
        return indices;
    }
    
    public int[][] GetCurrent3By4()
    {
        return current3By4;
    }
    
    #region SpinResult
    [System.Serializable]
    public struct SpinResult
    {
        public float rtp;
        public int reel1Index;
        public int reel2Index;
        public int reel3Index;
        public int reel4Index;
        public bool jackpotTriggered;
        /*
         * At the resolution of each spinner i, we have the list of 3 ints indicating:
         *  - At index 0: # of inf (jackpot) symbols on the middle row
         *  - At index 1: # of doubles on the board so far
         *  - At index 2: # of triples on the board so far
         *
         * Such that when the third spin resolves, we can go into ComboProgressAtReelResolveX[2],
         * and deduce the progress for the combo indicators
         */
        public List<List<int>> ComboProgressAtReelResolveX;
        /*
         * Nested array containing 2 variable len arrays containing the indices of the reels that
         * generated the most $.
         * Each number inside here will be 0-11 where 0 is the top left and 11 is the bottom right target.
         */
        public List<List<int>> keyIndices;
        public float newBalance;
        public int instanceId;

        public SpinResult(float win, int index1, int index2, int index3, int index4, bool jackpot, List<List<int>> comboProgressMatrix, List<List<int>> keyTargets)
        {
            rtp = win;
            reel1Index = index1;
            reel2Index = index2;
            reel3Index = index3;
            reel4Index = index4;
            jackpotTriggered = jackpot;
            newBalance = -1f;
            ComboProgressAtReelResolveX = comboProgressMatrix;
            keyIndices = keyTargets;
            instanceId = MAUnityManager.UnityInstanceId;
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
