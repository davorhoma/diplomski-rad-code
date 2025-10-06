using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDicePositioner : MonoBehaviour
{
    private Vector3 rollPosition;
    [SerializeField] private Transform cornerParent;
    [SerializeField] private Transform cornerPosition;
    private float x = 0f;

    private void Start()
    {
        rollPosition = transform.position;
    }

    public void PositionDiceForRoll(List<Rigidbody> diceRbs)
    {
        float a = 3.5f - diceRbs.Count * 0.5f;
        foreach (var rb in diceRbs)
        {
            rb.useGravity = false;
            rb.transform.position = new Vector3(rollPosition.x + a, rollPosition.y, rollPosition.z);
            a += 1f;
        }
    }

    public void RetreatDice(Rigidbody[] rbs)
    {
        var pos = cornerPosition.position;
        foreach (var rb in rbs)
        {
            rb.transform.SetParent(cornerParent, true);
            rb.transform.position = pos;
            pos.x += 1f;
        }
    }
}
