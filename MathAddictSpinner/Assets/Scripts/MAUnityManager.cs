using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * Puts everything together, managing spins and UI and marrying them together into a not very happy couple,
 * but at least it's better than being alone I think?
 */
public class MAUnityManager : MonoBehaviour
{
    // This manager is a singleton
    public static MAUnityManager Instance;
    
    // set on GameObject
    [SerializeField] private Spinners slotManager;
    [SerializeField] private UIDisplayer uiManager;
    
    // stored by the game manager for ease of access by other objects
    public List<List<int>> reels;  // 1 through 4

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        SetupSessionReels();
    }
    
    private void Start()
    {
        if (!slotManager || !uiManager)
        {
            Debug.LogError($"One of {nameof(slotManager)} or {nameof(uiManager)} is null!");
        }
    }

    public void OnStartTriggered()
    {
        // signal to activate this method comes from JS -- when on a problem page
        uiManager.SwitchScreens(toSlots: true);
    }
    
    public void OnBackTriggered()
    {
        // signal to activate this method comes from JS -- when exiting a problem page
        uiManager.SwitchScreens(toSlots: false);
        // result the UI to defaults
        uiManager.ResetToDefaults();
    }

    public void OnSpinTriggered()
    {
        // TODO: Fetch wager to be played from MA JS
        float spinWager = Random.Range(0f, 5f);
        
        // trigger math
        Spinners.SpinResult resultNumbers = slotManager.TriggerSpin(spinWager);
        // int[][] result3By4 = slotManager.GetCurrent3By4();  -- for debugging!
        
        // show results (dramatically if possible)
        uiManager.SetResult(resultNumbers);
        
        // add to wallet (dramatically if possible)
        // TODO: Insert here a command to send reward data to MA JS
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    #region Setup
    
    // called by MAUnityManager
    private void SetupSessionReels()
    {
        var elements = SpinnerConstants.reelSetup;
        var reel1 = ShuffleReel(elements);
        var reel2 = ShuffleReel(reel1);
        var reel3 = ShuffleReel(reel2);
        var reel4 = ShuffleReel(reel3);
        
        reels = new List<List<int>>()
        {
            reel1, reel2, reel3, reel4
        };
    }

    // fisher-yates shuffle
    private List<int> ShuffleReel(List<int> elements)
    {
        List<int> result = new List<int>(elements);
        
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }

        return result;
    }
    #endregion
}