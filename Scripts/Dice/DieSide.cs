using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieSide : MonoBehaviour
{
    private bool _onGround;
    public bool OnGround => _onGround;
    public int sideValue;

    private void OnTriggerStay(Collider col)
    {
        if (col.tag == "RollingPad")
        {
            _onGround = true;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.tag == "RollingPad")
        {
            _onGround = false;
        }
    }
}
