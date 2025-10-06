using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TClickTile : MonoBehaviour
{
    [SerializeField] private TTransparencyManager _tTransparencyManager;
    [SerializeField] private int _tutorialStep;
    [SerializeField] private TSelectOutline _tSelectOutline;
    [SerializeField] private string _nextDiceSelectionGroupName;

    private Image _image;

    private void Start()
    {
        _image = GetComponent<Image>();
    }

    private void OnMouseDown()
    {
        Debug.Log("Image on mouse down");
        if (_tTransparencyManager.CurrentStep == _tutorialStep)
        {
            PlaceDice();
        }
    }

    public void PlaceDice()
    {
        Debug.Log("Place dice on wagon");
        if (_tTransparencyManager.CurrentStep != _tutorialStep) return;

        var selectedDiceGroup = _tSelectOutline.SelectedDiceGroup;

        Vector3 pos = _image.rectTransform.position;

        pos.x -= 2.5f;
        pos.y = 1f;
        //pos.z -= 1.5f;
        pos.z += -1.5f;
        //_selectedDiceGroup.transform.position = pos;

        foreach (Transform child in selectedDiceGroup.transform)
        {
            Debug.Log("Moving 1 cube");
            child.GetComponent<Rigidbody>().MovePosition(pos);
            pos.x += 1f;
        }

        _tTransparencyManager.NextStep();
        _tSelectOutline.CurrentPossibleSelectionGroup = _nextDiceSelectionGroupName;
    }
}
