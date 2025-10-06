using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Players : NetworkBehaviour
{
    public static Players Instance;

    public List<Rigidbody> blueRigidBodies;
    public List<Rigidbody> redRigidBodies;
    public List<Rigidbody> greenRigidBodies;
    public List<Rigidbody> yellowRigidBodies;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    public List<Rigidbody> GetRigidBodies(PlayerColor player)
    {
        if (player == PlayerColor.BLUE)
        {
            //print("Returning blue rigidbodies");
            return blueRigidBodies;
        }
        else if (player == PlayerColor.RED)
        {
            //print("Returning red rigidbodies");
            return redRigidBodies;
        }
        else if (player == PlayerColor.GREEN)
        {
            //print("Returning green rigidbodies");
            return greenRigidBodies;
        }
        else 
        {
            //print("Returning yellow rigidbodies");
            return yellowRigidBodies;
        }
    }

    [Server]
    public void SetTag(Transform transform, string tag)
    {
        RpcSetChildTag(transform, tag);
    }

    [ClientRpc]
    private void RpcSetChildTag(Transform child, string tag)
    {
        child.tag = tag;
    }
}
