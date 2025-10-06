using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeUtil : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static IEnumerator Wait(float time = 0.5f)
    {
        yield return new WaitForSeconds(time);
    }
}
