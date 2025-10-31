using System;
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
    [SerializeField] private TextMeshProUGUI spinButtonText; 
    [SerializeField] private TextMeshProUGUI wagerText;
    [SerializeField] private TextMeshProUGUI spinOutcomeText;
    [SerializeField] private TextMeshProUGUI lastWinText;
    [SerializeField] private TextMeshProUGUI balanceText;
    #endregion

    private List<int> reelIndexes = new List<int>{ 1, 1, 1, 1};  // start at 1
    private int spinCoroutineCounter = 0;
    
    private void Start()
    {
        if (!startScreen || !slotScreen || !wagerText || !spinOutcomeText || !lastWinText || !spinButtonText || !balanceText || orderedReelTextObjects.Count < 12)
        {
            Debug.LogError($"UI properties are null. Check the GameObject {this.name}!");
        }
        
        spinOutcomeText?.SetText(UIConstants.onHoldText);
        wagerText?.SetText($"{UIConstants.wagerText}00.00");
        lastWinText?.SetText($"{UIConstants.lastWinText}00.00");
        SetSpinButtonInteractable(false);
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

    public void SetWager(float wager)
    {
        wagerText?.SetText($"{UIConstants.wagerText}{Math.Truncate(100 * wager) / 100}");
    }
    
    public void SetLastWin(float lastWin)
    {
        lastWinText?.SetText($"{UIConstants.lastWinText}{Math.Truncate(100 * lastWin) / 100}");
    }

    public void SetBalance(float balance)
    {
        balanceText?.SetText($"${Math.Truncate(100 * balance) / 100}");
    }
    
    private IEnumerator AnimateSlotSpin(Spinners.SpinResult resultNumbers)
    {
        // prep to start animations
        SetSpinButtonInteractable(false);
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
        reelIndexes[0] = resultNumbers.reel1Index;
        reelIndexes[1] = resultNumbers.reel2Index;
        reelIndexes[2] = resultNumbers.reel3Index;
        reelIndexes[3] = resultNumbers.reel4Index;
        // its important we don't set the spin button as interactable here again
        // as its interact-ability is controlled by the wagers computed and sent
        // to Unity by JS
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
        double roundedRtp = Math.Truncate(100 * resultNumbers.rtp) / 100;
        if (resultNumbers.jackpotTriggered)
        {
            resultString += $"{UIConstants.jackpotText}\n {roundedRtp}";
        } 
        else if (resultNumbers.rtp > 0)
        {
            resultString += $"{UIConstants.successText}\n {roundedRtp}";
        }
        else
        {
            resultString += $"{UIConstants.lossText}";
        }
        spinOutcomeText?.SetText(resultString); 
        SetLastWin(resultNumbers.rtp);
    }

    public void ResetToDefaults()
    {
        reelIndexes = new List<int>{ 1, 1, 1, 1};
        spinCoroutineCounter = 0; 
        SetSpinToWinText();
    }

    public void SetSpinToWinText()
    {
        spinOutcomeText?.SetText(UIConstants.onHoldText);
    }

    public void SetSpinButtonInteractable(bool interactable)
    {
        if (spinButton != null)
        {
            spinButton.interactable = interactable;
            // set transparency of text based on interact-ability
            Color spinButtonTextColor = spinButtonText.color;
            spinButtonTextColor.a = interactable ? 1.0f : 0.0f;
            spinButtonText.color = spinButtonTextColor;
            
            // show the user they can spin through an animation
            // TODO!
        }
    }
}
