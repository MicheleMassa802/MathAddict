using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    // SET ON EDITOR
    public List<Graphic> graphicComponents = new List<Graphic>();
    public TextMeshProUGUI textComponent;  // optional customizable text component for popup
    
    // UI-related constants
    private const float fadeInDuration = 0.15f;
    public float holdDuration = 3.5f;
    private const float fadeOutDuration = 1.0f;

    private const float startAlpha = 0f;
    private const float endAlpha = 1f;
    public float awayY = 2000f;
    public float targetY = 25f;

    public float inactiveScaleFactor = 0.2f;
    public float activeScaleFactor = 0.8f;
    private Vector3 inactiveScale = Vector3.one;
    private Vector3 activeScale = Vector3.one;
    private Vector3 awayPosition;
    private Vector3 inactivePosition;
    private Vector3 activePosition;
    
    private void Start()
    {
        inactiveScale *= inactiveScaleFactor;
        activeScale *= activeScaleFactor;
        transform.localScale = inactiveScale;
        SetGraphicTransparency(startAlpha);
        inactivePosition = transform.localPosition;
        activePosition = inactivePosition + new Vector3(0f, targetY, 0f);
        awayPosition = inactivePosition + new Vector3(0f, awayY, 0f);
        transform.localPosition = awayPosition;
    }
    
    /*
     * popupData in form:
     * "averageSecondsPerLessonInt:percentageDeltaInt:distractionCountInt:countDeltaInt"
     */
    public void TriggerPopup(string popupData = "")
    {
        if (popupData != "" && textComponent != null)
        {
            // parse string and set data on popup
            string[] fourInts = popupData.Split(':');
            if (fourInts.Length != 4)
            {
                Debug.LogError($"Array sent by JS for end of lesson popup is of len != 4: {fourInts.Length}");
                return;
            }

            textComponent.text = SetLessonPopupTextFromFloats(
                int.Parse(fourInts[0]), int.Parse(fourInts[1]), int.Parse(fourInts[2]), int.Parse(fourInts[3])
                );
        }
        StartCoroutine(PlaySequence());
    }

    private string SetLessonPopupTextFromFloats(int timeSeconds, int timeDeltaPercent, int distractionCount, int distractionCountDelta)
    {
        string timeDeltaString = timeDeltaPercent < 0 ? "slower" : "faster";
        string distractionCountDeltaString = distractionCount < 0 ? "less" : "more";
        
        string formattedTime = TimeSpan.FromSeconds(timeSeconds).ToString(@"mm\:ss");
            
        string message =
            $"<b>Time: {formattedTime}</b>\n" + 
            $"{Math.Abs(timeDeltaPercent)}% {timeDeltaString} than previous available week's average!\n\n" + 
            $"<b>Distraction Count: {distractionCount}</b>\n" + 
            $"{Math.Abs(distractionCountDelta)} times {distractionCountDeltaString} than previous available week's average!";
        
        return message;
    }

    private IEnumerator PlaySequence()
    {
        gameObject.transform.localScale = inactiveScale;
        SetGraphicTransparency(startAlpha);
        transform.localPosition = inactivePosition;

        // fade, scale and move in
        float t = 0f;
        while (t < fadeInDuration)
        {
            float normalized = t / fadeInDuration;
            
            float alpha = Mathf.Lerp(startAlpha, endAlpha, normalized);
            SetGraphicTransparency(alpha);
            gameObject.transform.localScale = Vector3.Lerp(inactiveScale, activeScale, normalized);
            Vector3 innerPos = transform.localPosition;
            innerPos.y = Mathf.Lerp(inactivePosition.y, activePosition.y, normalized);
            gameObject.transform.localPosition = innerPos;
            
            t += Time.deltaTime;
            yield return null;
        }

        // snap to finish
        SetGraphicTransparency(endAlpha);
        gameObject.transform.localScale = activeScale;
        Vector3 pos = gameObject.transform.localPosition;
        pos.y = activePosition.y;
        gameObject.transform.localPosition = pos;

        // wait then get rid of popup
        if (holdDuration > 0)
        {
            yield return new WaitForSeconds(holdDuration);
        }

        // fade out
        t = 0f;
        while (t < fadeOutDuration)
        {
            float normalized = t / fadeOutDuration;
            float alphaEnd = Mathf.Lerp(endAlpha, 0f, normalized);
            SetGraphicTransparency(alphaEnd);

            t += Time.deltaTime;
            yield return null;
        }
        
        // set back to start parameters
        gameObject.transform.localScale = inactiveScale;
        SetGraphicTransparency(startAlpha);
        transform.localPosition = awayPosition;
    }
    
    private void SetGraphicTransparency(float alphaRatio)
    {
        foreach (var g in graphicComponents)
        {
            Color c = g.color;
            c.a = alphaRatio;
            g.color = c;
        }
    }
    
}
