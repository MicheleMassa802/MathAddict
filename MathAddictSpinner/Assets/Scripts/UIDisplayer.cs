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
    [SerializeField] private List<Image> orderedReelImageObjects;  // ordered from 11-13...1X-4X (12)
    
    // Miscellaneous
    [SerializeField] private TextMeshProUGUI betText;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private List<GameObject> spinFlowIndicators;  // correct answer, spin available, spin winner
    [SerializeField] private GameObject jackpotComboIndicator;
    [SerializeField] private GameObject multiply25ComboIndicator;
    [SerializeField] private GameObject multiply10ComboIndicator;

    
    // Spinner Sprites
    [SerializeField] private Sprite sigma;
    [SerializeField] private Sprite infinity;
    [SerializeField] private Sprite theta;
    [SerializeField] private Sprite pi;
    [SerializeField] private Sprite euler;
    [SerializeField] private Sprite z;
    [SerializeField] private Sprite y;
    [SerializeField] private Sprite x;
    
    #endregion

    private Dictionary<int, Sprite> symbolsMap;
    private List<int> reelIndexes = new List<int>{ 1, 1, 1, 1};  // start at 1
    private float elapsedCoroutineTime = 0;
    private Coroutine spinButtonPulse;
    private Coroutine spinAvailableFlash;
    
    private void Start()
    {
        if (!startScreen || !slotScreen || !betText || !winText || !balanceText 
            || orderedReelImageObjects.Count < 12)
        {
            Debug.LogError($"UI properties are null. Check the GameObject {this.name}!");
            return;
        }
        
        if (!sigma || !infinity || !theta || !pi || !euler || !z || !y || !x)
        {
            Debug.LogError($"UI sprites are null. Check the GameObject {this.name}!");
            return;
        }
        
        symbolsMap = new() 
        {
            {25, sigma},
            {18, infinity},
            {15, theta},
            {12, pi},
            {10, euler},
            {8,  z},
            {5,  y},
            {3,  x}
        };
        
        betText?.SetText("00.00");
        winText?.SetText("00.00");
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
        betText?.SetText($"{Math.Truncate(100 * wager) / 100}");
    }
    
    public void SetLastWin(float lastWin)
    {
        winText?.SetText($"{Math.Truncate(100 * lastWin) / 100}");
    }

    public void SetBalance(float balance)
    {
        balanceText?.SetText($"${Math.Truncate(100 * balance) / 100}");
    }
    
    private IEnumerator AnimateSlotSpin(Spinners.SpinResult resultNumbers, int wagersQueueLen, SoundSystem soundSystem, float timeDelta)
    {
        // prep to start animations
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
        
        // make sure lanes settle (sometimes depending on timing, that 4th reel could not settle)
        for (int i = 0; i < settledLanes.Count; i++)
        {
            if (!settledLanes[i])
            {
                SetReelTriplet(i + 1, resultIndices[i]);
            }
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
            orderedReelImageObjects[ (reelNumber - 1) * 3 + row].sprite = symbolsMap[symbol];
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
        // TODO MICHELE: SET OFF ACTIVATION LIGHTS FOR COMBOS FROM HERE
        SetLastWin(resultNumbers.rtp);
    }

    public void FlashSpinFlowIndicator(int index)
    {
        if (index == 1)
        {
            if (spinAvailableFlash != null)
            {
                // player has triggered spin manually!
                StopCoroutine(spinAvailableFlash);
                spinAvailableFlash = null;
            }
            else
            {
                // need to flash indefinitely until player triggers spin
                spinAvailableFlash = StartCoroutine(AnimateSpinFlowIndicators(spinFlowIndicators[index], UIConstants.spinIndicatorFlashLength[index], true));
                return;
            }
        }
 
        StartCoroutine(AnimateSpinFlowIndicators(spinFlowIndicators[index], UIConstants.spinIndicatorFlashLength[index]));
    }

    public void ResetToDefaults()
    {
        reelIndexes = new List<int>{ 1, 1, 1, 1};
        elapsedCoroutineTime = 0; 
    }

    // TODO MICHELE: recycle this to pulse the full screen when they can tap the reels to spin
    // private void ToggleSpinButtonAnimation(bool startAnimation)
    // {
    //     if (spinButtonPulse != null && !startAnimation)
    //     {
    //         StopCoroutine(spinButtonPulse);
    //         spinButtonPulse = null;
    //         spinButtonImage.color = new Color32(200, 200, 200, 155);
    //     }
    //     else if (spinButtonPulse == null &&  startAnimation)
    //     {
    //         spinButtonPulse = StartCoroutine(AnimateButtonClickable(spinButtonImage, new Color32(164, 105, 40, 100)));
    //     }
    // }

    private float ComputeSpinTime(float timeDelta)
    {
        const float lbLn = 3.219f;
        const float ubLn = 5.704f;
        
        // time delta can be anything from 0s -> +inf, so we clamp between 25s and 300s to avoid absurd values
        // I define the log of these values as constants here for efficiency's sake
        timeDelta = Mathf.Clamp(timeDelta, 25f, 300f);

        // map input to outputs through log scaling, and then map that back between 5 and 10 seconds
        float rate = (Mathf.Log(timeDelta) - lbLn) / (ubLn - lbLn);
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

    // toggle the gameObject on and off to make it flash
    private IEnumerator AnimateSpinFlowIndicators(GameObject indicator, float timeDelta, bool spinIndefinitely = false)
    {
        float timeElapsed = 0.0f;
        float flashDelay;
        
        while (timeElapsed < timeDelta)
        {
            indicator.SetActive(!indicator.activeSelf);
            flashDelay = spinIndefinitely ? 0.2f : Mathf.Lerp(0.1f, 0.25f, timeElapsed / timeDelta);
            yield return new WaitForSeconds(flashDelay);
            
            timeElapsed += Time.deltaTime;
        }
        
        indicator.SetActive(true);
    }
    #endregion
}
