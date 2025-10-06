using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Steal : MonoBehaviour
{
    //private AddEventTrigger _addEventTrigger;
    [SerializeField] PointCounter _pointCounterScript;
    [SerializeField] private ImageAssigner _imageAssigner;
    [SerializeField] private ClickTileEvent _clickTileScript;

    public void StealTile()
    {
        _imageAssigner.FindTilesForSteal();
        _clickTileScript.enabled = true;
        _clickTileScript.SetAction(PlayerAction.STEAL);
    }
}
