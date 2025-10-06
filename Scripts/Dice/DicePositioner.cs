using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DicePositioner : MonoBehaviour
{
    private Vector3 rollPosition;
    [SerializeField] private GameObject rollPositionObj;

    private void Start()
    {
        rollPosition = transform.position;
    }

    public void PositionDiceForRoll(List<Rigidbody> diceRbs)
    {
        //Debug.Log("POSITION DICE SIZE: " + diceRbs.Count);
        float a = 3.5f - diceRbs.Count * 0.5f;
        foreach (var rb in diceRbs)
        {
            rb.useGravity = false;
            rb.transform.position = new Vector3(rollPosition.x + a, rollPosition.y, rollPosition.z);
            //rb.MovePosition(new Vector3(rollPosition.x + a, rollPosition.y, rollPosition.z));
            //rb.position = new Vector3(rollPosition.x + a, rollPosition.y, rollPosition.z);
            a += 1f;
        }
        //Debug.Log("POSITIONED");
    }
}
