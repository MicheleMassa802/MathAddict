using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*
 * Puts everything together, managing spins and UI and marrying them together into a not very happy couple,
 * but at least its better than being alone I think?
 */
public class MAUnityManager : MonoBehaviour
{
    // set on GameObject
    [SerializeField] private Spinners slotManager;
    [SerializeField] private UIDisplayer uiManager;
    
    private void Start()
    {
        if (!slotManager || !uiManager)
        {
            Debug.LogError($"One of {nameof(slotManager)} or {nameof(uiManager)} is null!");
        }
    }

    public void OnSpinTriggered()
    {
        // fetch wager from JS
        float spinWager = Random.Range(0f, 5f);
        
        // trigger math
        Spinners.SpinResult resultNumbers = slotManager.TriggerSpin(spinWager);
        int[][] result3By4 = slotManager.GetCurrent3By4();
        
        // show results (dramatically if possible)
        uiManager.SetResult(resultNumbers, result3By4);
        
        // add to wallet (dramatically if possible)
        
    }
}