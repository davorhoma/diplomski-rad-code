using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenChest : MonoBehaviour
{
    [SerializeField] private GameObject _pivotObject;
    private float rotationSpeed = 1f;

    private void Update()
    {
        if (Input.GetKey(KeyCode.C))
        {
            transform.RotateAround(_pivotObject.transform.position, new Vector3(1, 0, 0), rotationSpeed);
        }
    }
}
