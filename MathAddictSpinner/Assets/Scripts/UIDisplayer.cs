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
}
