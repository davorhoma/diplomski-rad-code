using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrTDiceClick : MonoBehaviour
{
    [SerializeField] private int _tutorialStep;
    [SerializeField] private TrTTransparencyManager _trTTransparencyManager;

    private void OnMouseDown()
    {
        Debug.Log("Click on die");
        if (_trTTransparencyManager.CurrentStep == _tutorialStep)
        {
            _trTTransparencyManager.NextStep();
        }
    }
}
