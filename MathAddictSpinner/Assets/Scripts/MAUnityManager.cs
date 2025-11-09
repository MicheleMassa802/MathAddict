using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Runtime.InteropServices;

/*
 * Puts everything together, managing spins and UI and marrying them together into a not very happy couple,
 * but at least it's better than being alone I think?
 */
public class MAUnityManager : MonoBehaviour
{
    // This manager is a singleton
    public static MAUnityManager Instance;
    
    private Queue<float> wagers = new();
    private float balance = 0f;
    
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
        #if UNITY_EDITOR
        wagers.Enqueue(Random.Range(1f, 5f));
        #endif
        uiManager.SetSpinButtonInteractable(wagers.Count > 0);
    }
    
    private void Start()
    {
        if (!slotManager || !uiManager)
        {
            Debug.LogError($"One of {nameof(slotManager)} or {nameof(uiManager)} is null!");
        }
    }

    public void OnSpinTriggered()
    {
        float currWager;
        
        #if UNITY_EDITOR
        wagers.Enqueue(Random.Range(1f, 5f));
        #endif
        
        if (wagers.Count > 0) {
            currWager = wagers.Dequeue();
        }
        else 
        {
            // this shouldn't happen, but handle it by setting the text as if a wager isn't present
            Debug.LogError("Attempted to trigger spin without a wager available!");
            uiManager.SetSpinButtonInteractable(false);
            return;
        }
        
        uiManager.SetWager(currWager);
        uiManager.SetSpinToWinText();
        
        // trigger math
        Spinners.SpinResult resultNumbers = slotManager.TriggerSpin(currWager);
        // update the balance for the JS message and UI
        resultNumbers.newBalance = balance + resultNumbers.rtp;
        balance = resultNumbers.newBalance;
        
        // show results, send in the updated wagers count to decide if the button stays interactable
        uiManager.SetResult(resultNumbers, wagers.Count);
        
        // add to wallet (dramatically if possible)
        ParseAndSendResult(resultNumbers);
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
    #endif
    
    private void ParseAndSendResult(Spinners.SpinResult resultNumbers)
    {
        string resultJson = JsonUtility.ToJson(resultNumbers);
        Debug.Log($"Simulating send to JS: {resultJson}");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        SendResults(resultJson);
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
    
    public void SetWager(string jsWager)
    {
        // this method is called by JS when a question is completed, which then allows the player to spin
        // using the wagers they've accumulated in the wager queue
        Debug.Log($"Received Wager: {jsWager}");
        float realWager = float.Parse(jsWager);
        
        // by default we set the wager that then gets triggered by our spin!
        wagers.Enqueue(realWager);
        uiManager.SetWager(realWager);
        uiManager.SetSpinButtonInteractable(wagers.Count > 0);
    }
    
    public void SetBalance(string jsBalance)
    {
        // this method is called by JS when starting a session to setup the balance in game.
        Debug.Log($"Received Balance: {jsBalance}");
        float realBalance = float.Parse(jsBalance);
        
        balance = realBalance;
        uiManager.SetBalance(balance);
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