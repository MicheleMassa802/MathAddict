using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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
    [SerializeField] private Image spinButtonImage;
    
    // Miscellaneous
    [SerializeField] private TextMeshProUGUI spinButtonText; 
    [SerializeField] private TextMeshProUGUI wagerText;
    [SerializeField] private TextMeshProUGUI spinOutcomeText;
    [SerializeField] private TextMeshProUGUI lastWinText;
    [SerializeField] private TextMeshProUGUI balanceText;
    #endregion

    private List<int> reelIndexes = new List<int>{ 1, 1, 1, 1};  // start at 1
    private float elapsedCoroutineTime = 0;
    private Coroutine spinButtonPulse;
    
    private void Start()
    {
        if (!startScreen || !slotScreen || !wagerText || !spinOutcomeText || !lastWinText || !spinButtonText ||
            !spinButtonImage || !balanceText || orderedReelTextObjects.Count < 12)
        {
            Debug.LogError($"UI properties are null. Check the GameObject {this.name}!");
        }
        
        spinOutcomeText?.SetText(UIConstants.onHoldText);
        wagerText?.SetText($"{UIConstants.wagerText}00.00");
        lastWinText?.SetText($"{UIConstants.lastWinText}00.00");
    }

    public void SwitchScreens(bool toSlots)
    {
        startScreen.SetActive(!toSlots);
        slotScreen.SetActive(toSlots);
    }

    public void SetResult(Spinners.SpinResult resultNumbers, int wagersQueueLen, SoundSystem soundSystem, float timeDelta)
    {
        // start spin animation from current indexes up to result indexes
        StartCoroutine(AnimateSlotSpin(resultNumbers, wagersQueueLen, soundSystem, timeDelta));
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
    
    private IEnumerator AnimateSlotSpin(Spinners.SpinResult resultNumbers, int wagersQueueLen, SoundSystem soundSystem, float timeDelta)
    {
        // prep to start animations
        SetSpinButtonInteractable(false);
        elapsedCoroutineTime = 0;
        soundSystem.PlaySpinSound();
        
        float spinDuration = ComputeSpinTime(timeDelta);
        List<float> counterDivisors = SpinnerConstants.GetReelSpinsDivisors(spinDuration);
        List<float> spinLimits = SpinnerConstants.GetReelSpinsLimits(spinDuration);
        List<bool> settledLanes = new () {false, false, false, false};
        List<int> resultIndices = new List<int>
            { resultNumbers.reel1Index, resultNumbers.reel2Index, resultNumbers.reel3Index, resultNumbers.reel4Index };
        int len = SpinnerConstants.reelLength;
        
        // go through the X seconds of spin
        while (elapsedCoroutineTime < spinDuration)
        {
            for (int i = 0; i < counterDivisors.Count; i++)
            {
                if (elapsedCoroutineTime % counterDivisors[i] != 0 && elapsedCoroutineTime < spinLimits[i])
                {
                    // spin the corresponding reel
                    SetReelTriplet(i + 1, (reelIndexes[i] + (int)(elapsedCoroutineTime * 60)) % len);
                } 
                else if (!settledLanes[i] && elapsedCoroutineTime >= spinLimits[i])
                {
                    // settle down on the true values
                    SetReelTriplet(i + 1, resultIndices[i]);
                    settledLanes[i] = true;  // avoid settling multiple times
                }
            }
            
            elapsedCoroutineTime += Time.deltaTime;
            yield return null;
        }
        
        // clean up
        DisplayResultText(resultNumbers);
        if (resultNumbers.rtp > 0)
        {
            soundSystem.PlayWinSound(resultNumbers.jackpotTriggered);
        }
        else
        {
            soundSystem.PlayLoseSound();
        }
        SetBalance(resultNumbers.newBalance);
        reelIndexes[0] = resultNumbers.reel1Index;
        reelIndexes[1] = resultNumbers.reel2Index;
        reelIndexes[2] = resultNumbers.reel3Index;
        reelIndexes[3] = resultNumbers.reel4Index;
        
        // depending on the wagers queue, the button could still be seen as interactable
        SetSpinButtonInteractable(wagersQueueLen > 0);
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
        elapsedCoroutineTime = 0; 
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
            ToggleSpinButtonAnimation(interactable);
        }
    }

    private void ToggleSpinButtonAnimation(bool startAnimation)
    {
        if (spinButtonPulse != null && !startAnimation)
        {
            StopCoroutine(spinButtonPulse);
            spinButtonPulse = null;
            spinButtonImage.color = new Color32(200, 200, 200, 155);
        }
        else if (spinButtonPulse == null &&  startAnimation)
        {
            spinButtonPulse = StartCoroutine(AnimateButtonClickable(spinButtonImage, new Color32(164, 105, 40, 100)));
        }
    }

    private float ComputeSpinTime(float timeDelta)
    {
        const float lbLn = 3.219f;
        const float ubLn = 5.704f;
        
        // time delta can be anything from 0s -> +inf, so we clamp between 25s and 300s to avoid absurd values
        // I define the log of these values as constants here for efficiency sake
        timeDelta = Mathf.Clamp(timeDelta, 25f, 300f);

        // map input to outputs through log scaling, and then map that back between 5 and 10 seconds
        float rate = (Mathf.Log(timeDelta) - lbLn) / (ubLn - lbLn);
        Debug.Log($"### Using timeDelta: {timeDelta}, we get a spinTime: {rate * 5f + 5f}");
        return rate * 5f + 5f;
    }

    #region UI Animations

    // Makes the buttonBorder provided pulse with a color pulsingColor
    private IEnumerator AnimateButtonClickable(Image buttonImage, Color32 pulsingColor)
    {
        var baseColor = new Color32(200, 200, 200, 155);
        var currColor = pulsingColor;
        float currAlpha = 155;
        float alphaRate = 155 * 0.25f;
        int minAlpha = 0, maxAlpha = 155;
        bool alphaIncreasing = false;
        while (true) 
        {
            currAlpha = alphaIncreasing ? currAlpha + alphaRate : currAlpha - alphaRate;
            if (currAlpha < minAlpha) 
            { 
                // switch color and start increasing alpha
                currColor = currColor.CompareRGB(baseColor) ? pulsingColor : baseColor;
                alphaIncreasing = true;
                currAlpha = minAlpha;
            } 
            else if (currAlpha > maxAlpha) 
            {
                // start decreasing alpha
                alphaIncreasing = false;
                currAlpha = maxAlpha;
            } 
            
            currColor.a = (byte)currAlpha;
            buttonImage.color = currColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion
}
