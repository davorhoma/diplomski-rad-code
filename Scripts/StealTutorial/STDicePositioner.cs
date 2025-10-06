using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class STDicePositioner : MonoBehaviour
{
    [SerializeField] private GameObject[] _blueDice;
    [SerializeField] private GameObject[] _redDice;
    [SerializeField] private Image[] _wagonImages;

    public void MoveDiceToCorner(Transform parent, int index)
    {
        float a = index - 1;
        Transform alreadyChild = parent.GetChild(0);

        for (int i = index; i < parent.childCount; i++)
        {
            var childRb = parent.GetChild(i).GetComponent<Rigidbody>();
            childRb.MovePosition(new Vector3(alreadyChild.position.x + a, alreadyChild.position.y, alreadyChild.position.z));
            a += 1f;
        }
    }
}
