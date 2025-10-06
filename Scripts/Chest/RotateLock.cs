using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLock : MonoBehaviour
{
    [SerializeField] private GameObject _pivotObject;
    private float rotationSpeed = 1f;

    private void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            transform.RotateAround(_pivotObject.transform.position, new Vector3(0, 1, 0), rotationSpeed);
        }
    }
}
