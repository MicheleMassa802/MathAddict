using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    public static int UnityInstanceId = 0;
    
    private Queue<Tuple<float, float>> wagers = new();
    private float balance = 0f;
    
    // set on GameObject
    [SerializeField] private Spinners slotManager;
    [SerializeField] private UIDisplayer uiManager;
    [SerializeField] private SoundSystem soundManager;
    
    // stored by the game manager for ease of access by other objects
    public List<List<int>> reels;  // 1 through 4
    private IEnumerator spinCoroutine;

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
        #if UNITY_EDITOR
        wagers.Enqueue(new Tuple<float, float>(Random.Range(1f, 5f), 25f));
        #endif
    }
    
    private void Start()
    {
        if (!slotManager || !uiManager || !soundManager)
        {
            Debug.LogError($"One of {nameof(slotManager)} or {nameof(uiManager)} is null!");
        }
    }

    public void OnSpinTriggered()
    {
        float currWager;
        float currTimeDelta;
        
        // #if UNITY_EDITOR
        // wagers.Enqueue(new Tuple<float, float>(Random.Range(1f, 5f), Random.Range(25f, 300f)));
        // #endif
        
        if (wagers.Count > 0) {
            var tuple = wagers.Dequeue();
            currWager = tuple.Item1;
            currTimeDelta = tuple.Item2;
        }
        else 
        {
            // this shouldn't happen, but handle it by setting the text as if a wager isn't present
            Debug.LogError("Attempted to trigger spin without a wager available!");
            return;
        }
        
        uiManager.SetWager(currWager);
        
        // trigger math
        Spinners.SpinResult resultNumbers = slotManager.TriggerSpin(currWager);
        // update the balance for the JS message and UI
        resultNumbers.newBalance = balance + resultNumbers.rtp;
        balance = resultNumbers.newBalance;
        
        // show results, send in the updated wagers count to decide if the button stays interactable
        uiManager.SetResult(resultNumbers, wagers.Count, soundManager, currTimeDelta);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    #region JS Interaction

    // Outgoing Methods
    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SendResults(string resultString);  
    // Takes in a string that encodes a result object into json for the extension to handle
    
    [DllImport("__Internal")]
    private static extern void SendToggleSound(string instanceString);  
    // Takes in a string representing the unity instance to send the message to in order to toggle its sound
    #endif
    
    public void ParseAndSendResult(Spinners.SpinResult resultNumbers)
    {
        string resultJson = JsonUtility.ToJson(resultNumbers);
        Debug.Log($"Simulating send to JS: {resultJson}");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        SendResults(resultJson);
        #endif
    }
    
    public void ParseAndToggleSound()
    {
        string resultJson = "{\"type\":\"toggleSound\",\"instanceId\":" + UnityInstanceId + "}";
        Debug.Log($"Simulating send to JS: {resultJson}");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        SendToggleSound(resultJson);
        #endif
    }
    
    // Incoming Methods -- Receive params as strings
    public void OnStartTriggered()  // also called by debug button
    {
        // signal to activate this method comes from JS -- when on a problem page
        uiManager.SwitchScreens(toSlots: true);
    }
    
    public void OnBackTriggered()  // also called by debug button
    {
        // signal to activate this method comes from JS -- when exiting a problem page
        uiManager.SwitchScreens(toSlots: false);
        // result the UI to defaults
        uiManager.ResetToDefaults();
    }

    // WARNING: ONLY TO BE CALLED BY UNITY FOR TESTING
    public void TestSetWager()
    {
        // randomly generate a wager and time to go through flow
        float wager = Random.Range(1f, 5f);
        float time = Random.Range(10f, 400f);
        SetWager($"{wager}:{time}");
    }
    
    public void SetWager(string jsWagerAndTime)
    {
        // this method is called by JS when a question is completed, which then allows the player to spin
        // using the wagers they've accumulated in the wager queue
        Debug.Log($"Received Wager:Time: {jsWagerAndTime}");
        
        string[] twoFloats = jsWagerAndTime.Split(':');
        if (twoFloats.Length != 2)
        {
            Debug.LogError($"Array sent by JS for wager setting is of len != 2: {twoFloats.Length}");
            return;
        }

        StartCoroutine(TriggerSpinFlow(float.Parse(twoFloats[0]), float.Parse(twoFloats[1])));
    }

    private IEnumerator TriggerSpinFlow(float realWager, float timeDelta)
    {
        if (spinCoroutine != null)
        {
            yield return spinCoroutine;
        }
        
        spinCoroutine = SpinFLowInternal(realWager, timeDelta);
        yield return spinCoroutine;
        spinCoroutine = null;
    }

    private IEnumerator SpinFLowInternal(float realWager, float timeDelta)
    {
        if (realWager < 0)
        {
            // incorrect answer, trigger negative stuff
            uiManager.FlashSpinFlowIndicator(0, false, soundManager);
            yield return new WaitForSeconds(UIConstants.spinIndicatorFlashLength[0]);
            
            soundManager.TurnMusicOff();
            soundManager.PlaySmallLoseSound();
            uiManager.CleanUpSpinFlowIndicators();
        }
        else
        {
            // if we are here it indicates a correct answer => activate indicator
            uiManager.SetTimeToAnswer((int)timeDelta);
            uiManager.FlashSpinFlowIndicator(0, true, soundManager);
            yield return new WaitForSeconds(UIConstants.spinIndicatorFlashLength[0]);
            
            // trigger spin
            wagers.Enqueue(new Tuple<float, float>(realWager, timeDelta));
            uiManager.FlashSpinFlowIndicator(2, true, soundManager);
            yield return new WaitForSeconds(UIConstants.spinIndicatorFlashLength[2]);
            
            /*
             * Note, we used to have a middle phase (index 1), but that has been removed, however
             * I was too lazy to actually remove it so its just hidden and gets skipped over here
             * in the process. Just don't get confused by the 0->2 index jump.
             */
            
            uiManager.SetWager(realWager);
            OnSpinTriggered();
        }
    }
    
    public void SetBalance(string jsBalance)
    {
        // this method is called by JS when starting a session to setup the balance in game.
        Debug.Log($"Received Balance: {jsBalance}");
        float realBalance = float.Parse(jsBalance);
        
        balance = realBalance;
        uiManager.SetBalance(balance);
    }
    
    public void ToggleSound(string noOpArg)
    {
        // this method is called by JS when toggling sound in the other unity instance
        Debug.Log($"Triggered Sound Toggle by JS on instance: {UnityInstanceId}");
        soundManager.ToggleSoundInternal("false");
    }
    
    public void SetInstanceId(string instanceId)
    {
        // this method is called by JS when starting a session to let the unity instance know its ID from
        // the POV of the JS controller
        Debug.Log($"Received InstanceId: {instanceId}");
        int id = int.Parse(instanceId);

        UnityInstanceId = id;
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