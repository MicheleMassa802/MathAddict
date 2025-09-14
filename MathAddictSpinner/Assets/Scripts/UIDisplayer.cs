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
    // set on GameObject
    [SerializeField] private TextMeshProUGUI spinOutcomeText;
    
    private void Start()
    {
        if (!spinOutcomeText)
        {
            Debug.LogError($"{nameof(spinOutcomeText)} is null!");
        }
        
        spinOutcomeText?.SetText(UIConstants.onHoldText);
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
