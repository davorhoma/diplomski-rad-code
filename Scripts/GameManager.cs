using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SyncVar] public List<Transform> _diceStartingPositions;
    [SerializeField] private List<GameObject> _allDicePrefabs;
    [SyncVar] private List<GameObject> _allDiceInstantiated = new List<GameObject>();
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

    private void Start()
    {
        // StartGame(Manager.GamePlayers.Count);
    }

    [Server]
    public void StartGame(int numOfPlayers)
    {
        NetworkServer.SpawnObjects();
        return;

        //Debug.Log("Start game game manager");
        //for (int i = 0; i < numOfPlayers; i++)
        //{
        //    //var spawnedDice = Instantiate(_dice[i]);
            
        //    var dice = _allDicePrefabs[i];
        //    int maxJ = 7*(i+1);
        //    var parent = _diceStartingPositions[i];
        //    var children = new List<Rigidbody>();
        //    for (int j = i * 7; j < maxJ; j++)
        //    {
        //        var cube = Instantiate(_allDicePrefabs[j]);
        //        var cubeRb = cube.GetComponent<Rigidbody>();
        //        cubeRb.isKinematic = false;
        //        NetworkServer.Spawn(cube.gameObject);
        //        //cube.transform.parent = parent;

        //        children.Add(cubeRb);

        //        //cube.transform.localScale = new Vector3(25, 25, 25);
        //        //parent.localScale = new Vector3(25, 25, 25);
        //        _allDiceInstantiated.Add(cube);
        //        //RpcSetParent(j, i);
        //    }

        //    //Set scripts
        //    if (i == 0)
        //    {
        //        ButtonRoll.Instance.BluePlayer = parent.gameObject;
        //        Players.Instance.blueRigidBodies.AddRange(children);
        //    }
        //    else if (i == 1)
        //    {
        //        ButtonRoll.Instance.RedPlayer = parent.gameObject;
        //        Players.Instance.redRigidBodies.AddRange(children);
        //    }
        //    else if (i == 2)
        //    {
        //        ButtonRoll.Instance.GreenPlayer = parent.gameObject;
        //        Players.Instance.greenRigidBodies.AddRange(children);
        //    }
        //    else if (i == 3)
        //    {
        //        ButtonRoll.Instance.YellowPlayer = parent.gameObject;
        //        Players.Instance.yellowRigidBodies.AddRange(children);
        //    }

        //    var applyForceScript = parent.GetComponent<ApplyForce>();
        //    applyForceScript.allRigidbodies.AddRange(children);
        //    children.Clear();
        //}
    }

    [ClientRpc]
    private void RpcSetParent(int j, int i)
    {
        if (_allDiceInstantiated[j].transform.parent != null) return;
        Debug.Log("Parent: " + _diceStartingPositions[i].name);
        _allDiceInstantiated[j].transform.SetParent(_diceStartingPositions[i], false);
        _allDiceInstantiated[j].transform.parent = _diceStartingPositions[i];
        //child.localScale = new Vector3(25, 25, 25);
    }

    [ClientRpc]
    private void RpcActivateDice(GameObject dice)
    {
        Debug.Log("ClientRpc Activate dice");
        dice.SetActive(true);
    }

    public string Capitalize(string word)
    {
        return word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower();
    }
}
