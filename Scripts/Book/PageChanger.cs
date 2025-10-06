using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageChanger : MonoBehaviour
{
    [SerializeField] private BookManager _bookManager;

    public void OnHalfRotation()
    {
        _bookManager.OnHalfRotation();
    }
}
