using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DiceClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (PlayerActionManager.Instance.GetAction() == PlayerAction.RETREAT)
        {
            RetreatDice();
            Debug.Log("Retreating!");
        }
    }

    private void RetreatDice()
    {
        if (!gameObject.tag.Contains("Positioned")) { return; }

        var script = NetworkClient.localPlayer.GetComponent<PlayerCommandController>();
        var objectNetId = gameObject.transform.parent.GetComponent<NetworkIdentity>().netId;
        Debug.Log("Parent net id: " + objectNetId);
        script.CmdRetreatDice(objectNetId);
    }
}
