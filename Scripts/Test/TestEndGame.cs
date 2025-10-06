using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEndGame : NetworkBehaviour
{
    [SerializeField] private GameObject[] _wagons;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RemoveAllWagons();
        }
    }

    void RemoveAllWagons()
    {
        var script = NetworkClient.localPlayer.GetComponent<PlayerCommandController>();
        if (script == null)
        {
            Debug.Log("Script == null");
        }

        script.CmdRemoveAllWagons(this);

        //foreach (var wagon in _wagons)
        //{
        //    wagon.SetActive(false);
        //}
    }

    [Server]
    public void ServerRemoveAllWagons()
    {
        foreach (var wagon in _wagons)
        {
            uint wagonToDeactivateId = wagon.GetComponent<NetworkIdentity>().netId;
            wagon.SetActive(false);

            RpcDisableGameObject(wagonToDeactivateId);
        }
    }

    [ClientRpc]
    private void RpcDisableGameObject(uint objectToDeactivateId)
    {
        if (NetworkClient.spawned.TryGetValue(objectToDeactivateId, out var deactivateIdentity))
        {
            deactivateIdentity.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Failed to find object with netId {objectToDeactivateId} to disable");
        }
    }
}
