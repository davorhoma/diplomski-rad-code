using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDiceClick : MonoBehaviour
{
    [SerializeField] private int _tutorialStep;
    [SerializeField] private TTransparencyManager _tTransparencyManager;

    private void OnMouseDown()
    {
        if (_tTransparencyManager.CurrentStep == _tutorialStep)
        {
            _tTransparencyManager.NextStep();
        }
    }
}
