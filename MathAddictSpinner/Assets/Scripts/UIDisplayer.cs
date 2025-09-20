using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * Manages the UI objects present in the game, mainly the displaying of results such as outcome text
 * and the displaying of the 3x4 section of the reels
 */
public class UIDisplayer : MonoBehaviour
{
    #region UI Properties set on the GameObject on the scene
    // Full Screen GameObjects
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject slotScreen;
    
    // Reels
    [SerializeField] private List<TextMeshProUGUI> orderedReelTextObjects;  // ordered from 11-13...1X-4X (12)
    [SerializeField] private Button spinButton; 
    
    // Miscellaneous
    [SerializeField] private TextMeshProUGUI spinOutcomeText;
    #endregion

    private List<int> reelIndexes = new List<int>{ 1, 1, 1, 1};  // start at 1
    private int spinCoroutineCounter = 0;
    
    private void Start()
    {
        if (!startScreen || !slotScreen || !spinOutcomeText || orderedReelTextObjects.Count < 12)
        {
            Debug.LogError($"UI properties are null. Check the GameObject {this.name}!");
        }
        
        spinOutcomeText?.SetText(UIConstants.onHoldText);
    }

    public void SwitchScreens(bool toSlots)
    {
        startScreen.SetActive(!toSlots);
        slotScreen.SetActive(toSlots);
    }

    public void SetResult(Spinners.SpinResult resultNumbers)
    {
        // start spin animation from current indexes up to result indexes
        StartCoroutine(AnimateSlotSpin(resultNumbers));
    }
    
    private IEnumerator AnimateSlotSpin(Spinners.SpinResult resultNumbers)
    {
        // prep to start animations
        spinButton.interactable = false;
        spinCoroutineCounter = 0;
        
        List<int> counterDivisors = SpinnerConstants.reelSpinsDivisors;
        List<int> spinLimits = GameConstants.reelSpinsLimits;
        List<int> resultIndices = new List<int>
            { resultNumbers.reel1Index, resultNumbers.reel2Index, resultNumbers.reel3Index, resultNumbers.reel4Index };
        int spinFrames = GameConstants.spinFrames;
        int len = SpinnerConstants.reelLength;
        
        // go through the 5 seconds of spin
        while (spinCoroutineCounter < spinFrames)
        {
            for (int i = 0; i < counterDivisors.Count; i++)
            {
                if (spinCoroutineCounter % counterDivisors[i] != 0 && spinCoroutineCounter < spinLimits[i])
                {
                    // spin the corresponding reel
                    SetReelTriplet(i + 1, (reelIndexes[i] + spinCoroutineCounter) % len);
                } 
                else if (spinCoroutineCounter == spinLimits[i])
                {
                    // settle down on the true values
                    SetReelTriplet(i + 1, resultIndices[i]);
                }
            }
            
            spinCoroutineCounter += 1;
            yield return null;
        }
        
        // clean up
        DisplayResultText(resultNumbers);
        spinButton.interactable = true;
        
        reelIndexes[0] = resultNumbers.reel1Index;
        reelIndexes[1] = resultNumbers.reel2Index;
        reelIndexes[2] = resultNumbers.reel3Index;
        reelIndexes[3] = resultNumbers.reel4Index;
    }
    
    // sets the items for a reel triplet for a frame of the animation
    private void SetReelTriplet(int reelNumber, int reelIndex)
    {
        // Note: reel number is 1 indexed
        
        // fetch reel
        List<int> currReel = MAUnityManager.Instance.reels[reelNumber - 1];
        int len = SpinnerConstants.reelLength;

        // set the text chars to their corresponding symbols
        int row = 0;
        for (int i = -1; i <= 1; i++)
        {
            int symbol = currReel[(reelIndex + i + len) % len];
            orderedReelTextObjects[ (reelNumber - 1) * 3 + row].text = SpinnerConstants.symbolsMap[symbol];
            row += 1;  // update row # for UI
        }
    }

    private void DisplayResultText(Spinners.SpinResult resultNumbers)
    {
        // show the text outcome
        string resultString = string.Empty;
        if (resultNumbers.jackpotTriggered)
        {
            resultString += $"{UIConstants.jackpotText}\n {resultNumbers.rtp}";
        } 
        else if (resultNumbers.rtp > 0)
        {
            resultString += $"{UIConstants.successText}\n {resultNumbers.rtp}";
        }
        else
        {
            resultString += $"{UIConstants.lossText}";
        }
        spinOutcomeText?.SetText(resultString);
    }

    public void ResetToDefaults()
    {
        reelIndexes = new List<int>{ 1, 1, 1, 1};
        spinCoroutineCounter = 0;
        spinOutcomeText?.SetText(UIConstants.onHoldText);
    }
}
