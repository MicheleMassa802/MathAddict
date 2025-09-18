using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    
    // Miscellaneous
    [SerializeField] private TextMeshProUGUI spinOutcomeText;
    #endregion

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

    public void SetResult(Spinners.SpinResult resultNumbers, int[][] result3By4)
    {
        // start spin animation
        
        // settle down on the resulting 3 by 4
        setReelTriplet(1, resultNumbers.reel1Index);
        setReelTriplet(2, resultNumbers.reel2Index);
        setReelTriplet(3, resultNumbers.reel3Index);
        setReelTriplet(4, resultNumbers.reel4Index);
        
        // display the 3x4 and show the outcome
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

    // sets the items for a reel triplet for a frame of the animation
    private void setReelTriplet(int reelNumber, int reelIndex)
    {
        // reel number is 1 indexed
        
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
}
