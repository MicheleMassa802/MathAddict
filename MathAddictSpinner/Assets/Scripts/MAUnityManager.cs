using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    
    private float currentWager = -1f;
    
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
        // trigger math
        Spinners.SpinResult resultNumbers = slotManager.TriggerSpin(currentWager);
        // int[][] result3By4 = slotManager.GetCurrent3By4();  -- for debugging!
        
        // show results (dramatically if possible)
        uiManager.SetResult(resultNumbers);
        
        // add to wallet (dramatically if possible)
        // TODO: Insert here a command to send reward data to MA JS
        
        // teardown
        currentWager = SpinnerConstants.invalidWager;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    #region JS Interaction

    // Outgoing Methods
    [DllImport("__Internal")]
    private static extern void SendResults(string resultString);  
    // TODO: takes in a string that encodes a result object into json for the extension to handle

    // Incoming Methods -- Receive params as strings
    public void SetWager(string jsWager)
    {
        // this method is called by JS when a question is completed, which then allows the player to spin
        // currentWager > 0 allows for the activation of the spin button
        float realWager = float.Parse(jsWager);
        if (currentWager > 0)
        {
            // previous spin was not triggered, do so and come back!
            OnSpinTriggered();
        }
        
        // by default we set the wager that then gets triggered by our spin!
        currentWager = realWager;
    } 
    #endregion
    
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