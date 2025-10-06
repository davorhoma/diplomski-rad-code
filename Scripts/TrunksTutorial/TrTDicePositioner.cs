using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrTDicePositioner : MonoBehaviour
{
    [SerializeField] private GameObject[] _blueDice;
    [SerializeField] private GameObject[] _redDice;
    [SerializeField] private Image[] _wagonImages;

    [SerializeField] private Transform _blueDie3;
    [SerializeField] private Rigidbody _dieToMoveRb;

    private void Start()
    {
        //StartingPositions();
    }

    private void StartingPositions()
    {
        Vector3 pos = _wagonImages[2].rectTransform.position;
        pos.x -= 2.5f;
        pos.y = -0.5f;
        pos.z += -1.5f;
        for (int i = 6; i >= 6; i--)
        {
            _blueDice[i].transform.GetComponent<Rigidbody>().MovePosition(pos);
            pos.x += 1f;
        }
    }
    public void MoveDiceToCorner()
    {
        var pos = _blueDie3.transform.position;

        _dieToMoveRb.MovePosition(new Vector3(pos.x + 1, pos.y, pos.z));
    }
}
