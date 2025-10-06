using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAspectRatioManager : MonoBehaviour
{
    void Start()
    {
        float aspect = (float)Screen.width / Screen.height;

        if (aspect < 1.7f)
            Camera.main.fieldOfView = 65f; // Adjust as needed
        else
            Camera.main.fieldOfView = 60f;
    }
}
