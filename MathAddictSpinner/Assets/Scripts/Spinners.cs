using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Spinners : MonoBehaviour
{
    [SerializeField] private float lowerBound = 0f;
    [SerializeField] private float upperBound = 1f;
    [SerializeField] private float successRate = 0.2f;
    
    [SerializeField] private string onHoldText = "Spin to win!";
    [SerializeField] private string successText = "WON!";
    [SerializeField] private string lossText = "LOST! \nTry Again!";
    
    public TextMeshProUGUI resultTextField;

    private void Start()
    {
        resultTextField.text = onHoldText;
    }
    
    // Random Spin Functions
    public void TriggerRandomSpin()
    {
        resultTextField.text = $"You've {(RandomSpinResult() ? successText : lossText)}";
    }
    
    private bool RandomSpinResult()
    {
        return Random.Range(lowerBound, upperBound) <= successRate;
    }
}
