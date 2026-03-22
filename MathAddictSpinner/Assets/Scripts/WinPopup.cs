using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinPopup : MonoBehaviour
{
    // SET ON EDITOR
    public List<Graphic> graphicComponents = new List<Graphic>();

    // UI-related constants
    private const float fadeInDuration = 0.15f;
    private const float holdDuration = 3.5f;
    private const float fadeOutDuration = 1.0f;
    
    private const float startAlpha = 20f / 255f;
    private const float endAlpha = 1;
    private const float awayY = 2000f;
    private const float targetY = 25f;
    
    private readonly Vector3 inactiveScale = Vector3.one * 0.2f;
    private readonly Vector3 activeScale = Vector3.one * 0.8f;
    private Vector3 awayPosition;
    private Vector3 inactivePosition;
    private Vector3 activePosition;
    
    private void Start()
    {
        transform.localScale = inactiveScale;
        SetGraphicTransparency(startAlpha);
        inactivePosition = transform.localPosition;
        activePosition = inactivePosition + new Vector3(0f, targetY, 0f);
        awayPosition = inactivePosition + new Vector3(0f, awayY, 0f);
        transform.localPosition = awayPosition;
    }
    
    public void TriggerWinPopup()
    {
        StartCoroutine(PlaySequence());
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
        yield return new WaitForSeconds(holdDuration);

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
