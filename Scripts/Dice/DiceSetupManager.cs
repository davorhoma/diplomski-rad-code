using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DiceSetupManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> _dicePrefabs;
    private CustomNetworkManager manager;

    public CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            else
            {
                return manager = CustomNetworkManager.singleton as CustomNetworkManager;
            }
        }
    }

    void Start()
    {
        if (isServer)
        {
            SpawnDice(Manager.GamePlayers.Count);
        }
    }

    [Server]
    public void SpawnDice(int playerCount)
    {
        GameObject blueParent = GameObject.Find("Blue");
        GameObject redParent = GameObject.Find("Red");
        GameObject greenParent = GameObject.Find("Green");
        GameObject yellowParent = GameObject.Find("Yellow");

        var children = new List<Rigidbody>();
        for (int i = 0; i < playerCount; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                GameObject cube = Instantiate(_dicePrefabs[i], GetSpawnPosition(i, j, playerCount), Quaternion.identity);
                NetworkServer.Spawn(cube);
                
                var cubeRb = cube.GetComponent<Rigidbody>();
                cubeRb.isKinematic = false;
                children.Add(cubeRb);
            }

            //Set scripts
            if (i == 0)
            {
                Players.Instance.blueRigidBodies.AddRange(children);
            }
            else if (i == 1)
            {
                Players.Instance.redRigidBodies.AddRange(children);
            }
            else if (i == 2)
            {
                Players.Instance.greenRigidBodies.AddRange(children);
            }
            else if (i == 3)
            {
                Players.Instance.yellowRigidBodies.AddRange(children);
            }

            children.Clear();
        }
    }

    private Vector3 GetSpawnPosition(int i, int j, int playerCount)
    {
        float spacing = 1f;
        int row = i;
        int col = j % 7;

        return new Vector3(-18f + col * spacing, 1f, -7.5f - row * spacing);
    }

    [Server]
    public void GroupDiceAfterRolling(PlayerColor currentPlayer)
    {
        Debug.Log("Grouping dice after rolling");
        switch (currentPlayer)
        {
            case PlayerColor.BLUE:
                RpcSetParents(0, "BlueSelectable");
                break;
            case PlayerColor.RED:
                RpcSetParents(0, "RedSelectable");
                break;
            case PlayerColor.GREEN:
                RpcSetParents(0, "GreenSelectable");
                break;
            case PlayerColor.YELLOW:
                RpcSetParents(0, "YellowSelectable");
                break;
        }
    }

    [ClientRpc]
    public void RpcSetParents(int startIndex, string gameObjectTag)
    {
        Debug.Log("RpcSetParents");
        if (isServer)
        {
            Debug.Log("Ovo je na hostu. Preskace se");
            return;
        }
        Debug.Log("RpcSetParents passed");

        string parentName = "";
        var found = GameObject.FindGameObjectsWithTag(gameObjectTag);
        var parents = new List<GameObject>();
        var dice = new List<GameObject>();
        foreach (var item in found)
        {
            if (item.transform.childCount == 0)
            {
                Debug.Log("Added parent");
                parents.Add(item);
            }
            else
            {
                Debug.Log("Added child cube");
                dice.Add(item);
            }
        }

        foreach (var cube in dice)
        {
            var gameDie = cube.GetComponent<GameDie>();
            switch (gameDie.Value)
            {
                case DieValue.JOKER:
                    parentName = "Joker";
                    break;
                case DieValue.HAT:
                    parentName = "Hat";
                    break;
                case DieValue.MASK:
                    parentName = "Mask";
                    break;
                case DieValue.BOOT:
                    parentName = "Boot";
                    break;
                case DieValue.GUN:
                    parentName = "Gun";
                    break;
                case DieValue.HORSESHOE:
                    parentName = "Horseshoe";
                    break;
            }

            foreach (var parent in parents)
            {
                if (parent.name.Contains(parentName))
                {
                    Debug.Log("Setting parrent");
                    cube.transform.SetParent(parent.transform, true);
                    break;
                }
            }
        }
    }

    [ClientRpc]
    public void RpcSetParent(uint childNetId, string parentName)
    {
        if (NetworkClient.spawned.TryGetValue(childNetId, out var childObj))
        {
            Debug.Log("parentName: " + parentName);
            var parent = GameObject.Find(parentName);
            if (childObj.transform.parent != parent)
            {
                childObj.transform.SetParent(parent.transform, true);
            }
        }
    }

    [ClientRpc]
    public void RpcSetParentWithNetId(uint childNetId, uint parentNetId)
    {
        if (NetworkClient.spawned.TryGetValue(childNetId, out var childObj) && NetworkClient.spawned.TryGetValue(parentNetId, out var parentObj))
        {
            var parent = parentObj.gameObject;
            Debug.Log("parentName: " + parent.name);
            if (childObj.transform.parent != parent)
            {
                childObj.transform.SetParent(parent.transform, true);
            }
        }
    }
}
