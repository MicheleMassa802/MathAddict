using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

/*
 * Manages the UI objects present in the game, mainly the displaying of results such as outcome text
 * and the displaying of the 3x4 section of the reels
 */
public class UIDisplayer : MonoBehaviour
{
    #region UI Properties set on the GameObject on the scene
    [SerializeField] private RectTransform canvasRectTransform;
    
    // Full Screen GameObjects
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject slotScreen;
    
    // Reels
    [SerializeField] private List<Image> orderedReelImageObjects;  // ordered from 11-13...1X-4X (12)
    private List<RectTransform> orderedReelImageObjectTransforms = new List<RectTransform>();
    
    // Miscellaneous
    [SerializeField] private TextMeshProUGUI betText;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private List<GameObject> spinFlowIndicators;  // correct answer, spin available, spin winner
    [SerializeField] private List<Image> filledComboIndicators;  // jackpot, 25x, 10x
    
    // Spinner Sprites
    [SerializeField] private Sprite sigma;
    [SerializeField] private Sprite infinity;
    [SerializeField] private Sprite theta;
    [SerializeField] private Sprite pi;
    [SerializeField] private Sprite euler;
    [SerializeField] private Sprite z;
    [SerializeField] private Sprite y;
    [SerializeField] private Sprite x;
    
    // Lines
    [SerializeField] private UILineRenderer lineRenderer1;
    [SerializeField] private UILineRenderer lineRenderer2;
    
    public WinPopup winPopupManager;
    #endregion

    private Dictionary<int, Sprite> symbolsMap;
    private List<int> reelIndexes = new List<int>{ 1, 1, 1, 1};  // start at 1
    private float elapsedCoroutineTime = 0;
    private Coroutine spinButtonPulse;
    
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

        foreach (var imageObject in orderedReelImageObjects)
        {
            orderedReelImageObjectTransforms.Add(imageObject.gameObject.GetComponent<RectTransform>());
        }
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

    public void SetTimeToAnswer(int seconds)
    {
        int minutes = seconds / 60;
        seconds %= 60;
        string minutesString = minutes < 10 ? "0" + minutes : minutes.ToString();
        timeText?.SetText($"{minutesString}:{seconds}");
    }
    
    private IEnumerator AnimateSlotSpin(Spinners.SpinResult resultNumbers, int wagersQueueLen, SoundSystem soundSystem, float timeDelta)
    {
        // prep to start animations
        ClearLines();
        ResetComboIndicators();
        elapsedCoroutineTime = 0;
        yield return new WaitForSeconds(0.25f);  // wait for a bit before the roll sound and animation
        soundSystem.PlaySpinSound();
        
        float spinDuration = ComputeSpinTime(timeDelta);
        List<float> counterDivisors = SpinnerConstants.GetReelSpinsDivisors(spinDuration);
        List<float> spinLimits = SpinnerConstants.GetReelSpinsLimits(spinDuration);
        List<bool> settledLanes = new () {false, false, false, false};
        List<int> resultIndices = new List<int>
            { resultNumbers.reel1Index, resultNumbers.reel2Index, resultNumbers.reel3Index, resultNumbers.reel4Index };
        int len = SpinnerConstants.reelLength;
        
        // set off flash indicator
        FlashSpinFlowIndicator(2, resultNumbers.rtp > 0, soundSystem, spinDuration);
        
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
                    // update comboIndicators
                    UpdateComboIndicators(resultNumbers.ComboProgressAtReelResolveX[i], soundSystem);
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
                UpdateComboIndicators(resultNumbers.ComboProgressAtReelResolveX[i], soundSystem);
            }
        }
        
        // clean up
        MAUnityManager.Instance.ParseAndSendResult(resultNumbers);
        DisplayResultText(resultNumbers);
        
        if (resultNumbers.rtp > 0)
        {
            soundSystem.TurnMusicOn();
            for (int i=0; i < resultNumbers.keyIndices.Count; i++)
            {
                DrawLine(resultNumbers.keyIndices[i], i);
            }
            soundSystem.PlayBigWinSound(resultNumbers.jackpotTriggered);
            winPopupManager.TriggerWinPopup();
        }
        else
        {
            soundSystem.TurnMusicOff();
            soundSystem.PlayBigLoseSound();
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

    private void UpdateComboIndicators(List<int> resultNumbersComboProgress, SoundSystem soundSystem)
    {
        int triples = resultNumbersComboProgress[2];
        int uniqueDoubles = resultNumbersComboProgress[1] - triples;
        int tripleDoubles = (uniqueDoubles > 2) ? 2 : uniqueDoubles;
        SetComboIndicatorFill(0, Mathf.Clamp(resultNumbersComboProgress[0]/4.0f, 0.0f, 1.0f), soundSystem);
        SetComboIndicatorFill(1, Mathf.Clamp(
            Mathf.Clamp(tripleDoubles/4.0f, 0.0f, 1.0f) + Mathf.Clamp(triples/4.0f, 0.0f, 1.0f),
            0.0f, 1.0f), soundSystem);
        SetComboIndicatorFill(2, Mathf.Clamp(resultNumbersComboProgress[1]/4.0f, 0.0f, 1.0f), soundSystem);
    }
    
    private void SetComboIndicatorFill(int indicatorIndex, float progress, SoundSystem soundSystem)
    {
        if (!Mathf.Approximately(filledComboIndicators[indicatorIndex].fillAmount, 1.0f) && Mathf.Approximately(progress, 1.0f))
        {
            soundSystem.PlayComboHitSound();
        }
        filledComboIndicators[indicatorIndex].fillAmount = progress;
    }

    private void ResetComboIndicators()
    {
        foreach (var indicator in filledComboIndicators)
        {
            indicator.fillAmount = 0f;
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

    public void FlashSpinFlowIndicator(int index, bool setIndicatorActive, SoundSystem soundSystem, float flashDurationOverride = -1.0f)
    {
        float flashDuration = flashDurationOverride < 0 ? UIConstants.spinIndicatorFlashLength[index] : flashDurationOverride;
        StartCoroutine(AnimateSpinFlowIndicators(spinFlowIndicators[index], flashDuration , setIndicatorActive, soundSystem));
    }

    public void CleanUpSpinFlowIndicators()
    {
        foreach (var indicator in spinFlowIndicators)
        {
            indicator.SetActive(false);
        }
    }

    public void ResetToDefaults()
    {
        reelIndexes = new List<int>{ 1, 1, 1, 1};
        elapsedCoroutineTime = 0; 
    }

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
    private IEnumerator AnimateSpinFlowIndicators(GameObject indicator, float timeDelta, bool setIndicatorActive, SoundSystem soundSystem)
    {
        float timeElapsed = 0.0f;
        float flashDelay;

        while (timeElapsed < timeDelta)
        {
            indicator.SetActive(!indicator.activeSelf);
            flashDelay = Mathf.Lerp(0.1f, 0.25f, timeElapsed / timeDelta);
            yield return new WaitForSeconds(flashDelay);
            
            timeElapsed += flashDelay;
        }

        indicator.SetActive(setIndicatorActive);
        if (setIndicatorActive)
        {
            soundSystem.PlaySmallWinSound();
        }
        else
        {
            soundSystem.PlaySmallLoseSound();
        }
        
        
    }

    private void DrawLine(List<int> winningTileIndices, int lineNumber)
    {
        var lineRenderer = lineNumber == 1 ? lineRenderer1 : lineRenderer2;
        if (winningTileIndices == null || winningTileIndices.Count == 0)
        {
            lineRenderer.Points = new Vector2[0];
            lineRenderer.SetVerticesDirty();
            return;
        }

        // get col-row from indices
        var sorted = winningTileIndices
            .OrderBy(i => i / 3)
            .ThenBy(i => i % 3)
            .ToList();

        // compute positions
        List<Vector2> uiPoints = new List<Vector2>();
        foreach (int index in sorted)
        {
            RectTransform tile = orderedReelImageObjectTransforms[index];
            uiPoints.Add(ToUILinePoint(tile, canvasRectTransform));
        }

        // render with lineRenderer
        lineRenderer.Points = uiPoints.ToArray();
        lineRenderer.SetVerticesDirty();
        StartCoroutine(BlinkLine(lineRenderer));
    }
    
    private IEnumerator BlinkLine(UILineRenderer line)
    {
        float elapsed = 0f;
        bool visible = true;
        var originalColor = line.color;

        while (elapsed < 3f)
        {
            visible = !visible;

            var c = originalColor;
            c.a = visible ? 1f : 0f;
            line.color = c;
            line.SetVerticesDirty();

            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }

        line.color = originalColor;
        line.SetVerticesDirty();
    }


    private void ClearLines()
    {
        lineRenderer1.Points = new Vector2[0];
        lineRenderer2.Points = new Vector2[0];
        lineRenderer1.SetVerticesDirty();
        lineRenderer2.SetVerticesDirty();
    }
    
    private Vector2 ToUILinePoint(RectTransform tile, RectTransform canvasRect)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, tile.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out Vector2 uiPos
        );

        return uiPos;
    }

    #endregion
}
