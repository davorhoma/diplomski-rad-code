using Mirror;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum MaterialColor { NONE = 0, GREEN = 1, RED = 2 };
public enum TrunkBack { HAT = 0, MASK = 1, HEEL = 2, HORSESHOE = 3, GUN = 4 };

public class ImageAssigner : NetworkBehaviour
{
    public Image[] imageObjects; // Reference to your Image GameObjects in the Unity Editor
    public Sprite[] imageSprites; // Array of sprites to assign to the images
    [SyncVar] int[] spriteIndices = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

    [SyncVar]
    private string assignedValue;

    [SerializeField] private GameObject[] _trunkBacks;
    [SerializeField] private GameObject[] _trunkFronts;
    private bool[] _trunkTaken = new bool[16];

    [SerializeField, FormerlySerializedAs("imageProperties")] Wagon[] _wagons;
    [SerializeField, FormerlySerializedAs("_boardProperties")] private Board _board;
    [SerializeField, FormerlySerializedAs("_currentPlayerScript")] private TurnManager _turnManager;
    [SerializeField] private PointCounter _pointCounterScript;

    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material redMaterial;


    [SyncVar] private List<int> _outlinedRed = new List<int>();
    [SyncVar] private List<int> _outlinedGreen = new List<int>();
    public List<int> OutlinedRed => _outlinedRed;

    [SerializeField] private Rotate[] _rotateScripts;
    [SerializeField] private GameObject _trunkGo;

    [SyncVar] public List<int> indexes = new List<int>();
    private GameObject _trunkToDestroy;
    public GameObject trunkToDestroy { get { return _trunkToDestroy; } set { _trunkToDestroy = value; } }
    [SerializeField] private TrunkPlacer _trunkPlacerScript;

    public override void OnStartServer()
    {
        base.OnStartServer();

        AssignValueOnServer();
        if (isServer)
        {
            Debug.Log("Pozvano sa SERVER-a");
            NotifyClientsToDisplayValue();
        }
        else if (isClient)
        {
            Debug.Log("Pozvano sa KLJENTA");
        }

        RpcIspisi();
    }

    [ClientRpc]
    public void RpcIspisi()
    {
        Debug.Log("Ispisao");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        //CmdInitializeImages();
        //CmdInitializeServerImages();
        //CmdInitializeImages();

        if (isOwned)
        {
            Debug.Log("IS OWNED");
        }
    }

    [Server]
    private void AssignValueOnServer()
    {
        assignedValue = "Haha";
        Debug.Log($"Server je postavio vrednost: {assignedValue}");
    }

    [ClientRpc]
    private void RpcDisplayValue()
    {
        Debug.Log($"Klijent je primio vrednost: {assignedValue}");
    }

    // Server obave?tava sve klijente da ispi?u vrednost
    [Server]
    private void NotifyClientsToDisplayValue()
    {
        RpcDisplayValue();
    }

    [TargetRpc]
    public void TargetSetImageMaterial(NetworkConnection targetClient, int index, MaterialColor materialColor)
    {
        Debug.Log("TargetSetImageMaterial");
        SetImageMaterial(index, materialColor);
    }

    public void SetImageMaterial(int index, MaterialColor materialColor)
    {
        if (materialColor == MaterialColor.GREEN)
            imageObjects[index].material = greenMaterial;
        else if (materialColor == MaterialColor.RED)
            imageObjects[index].material = redMaterial;
        else
            imageObjects[index].material = null;
    }

    public bool IsOutlined(int index)
    {
        for (int i = 0; i < _outlinedGreen.Count; i++)
            print(_outlinedGreen[i]);
        print(index);
        if (_outlinedGreen.Contains(index))
        {
            print("outlined green");
            return true;
        }
        else
        {
            print("not outlined green");
            return false;
        }
    }

    public void FindTilesForSteal()
    {
        print("finding");

        // Clear green outlines
        RemoveGreenOutlines();
        _outlinedRed.Clear();

        PlayerColor currentPlayer = _turnManager.Current;

        switch (currentPlayer)
        {
            case PlayerColor.BLUE:
                for (int i = 0; i < _wagons.Length; i++)
                {
                    if ((_wagons[i] != null) && _wagons[i].CanSteal(PlayerColor.BLUE))
                    {
                        print("Found red");
                        _outlinedRed.Add(i);

                        SetImageMaterial(i, MaterialColor.RED);
                    }
                }
                break;
            case PlayerColor.RED:
                for (int i = 0; i < _wagons.Length; i++)
                {
                    if ((_wagons[i] != null) && _wagons[i].CanSteal(PlayerColor.RED))
                    {
                        print("Found red");
                        _outlinedRed.Add(i);

                        SetImageMaterial(i, MaterialColor.RED);
                    }
                }
                break;
            case PlayerColor.GREEN:
                for (int i = 0; i < _wagons.Length; i++)
                {
                    if ((_wagons[i] != null) && _wagons[i].CanSteal(PlayerColor.GREEN))
                    {
                        print("Found red");
                        _outlinedRed.Add(i);

                        SetImageMaterial(i, MaterialColor.RED);
                    }
                }
                break;
            case PlayerColor.YELLOW:
                for (int i = 0; i < _wagons.Length; i++)
                {
                    if ((_wagons[i] != null) && _wagons[i].CanSteal(PlayerColor.YELLOW))
                    {
                        print("Found red");
                        _outlinedRed.Add(i);

                        SetImageMaterial(i, MaterialColor.RED);
                    }
                }
                break;
        }
    }

    [Server]
    public void ServerOutlineAvailableTile(string name, NetworkConnectionToClient client)
    {
        // Clear previous red/green outlines
        TargetRemoveRedOutlines(client);
        TargetRemoveGreenOutlines(client);
        _outlinedGreen.Clear();

        PlayerColor currentPlayer = _turnManager.Current;

        var newList = new List<int>();
        switch (currentPlayer)
        {
            case PlayerColor.BLUE:
                {
                    Debug.Log("_board.Blue = " + _board.Blue);
                    if (_board.Blue > 0)
                    {
                        if (name.Contains("Hat") && _board.hasBlueHats.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasBlueHats.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasBlueHats.TileNumber);
                            newList.Add(_board.hasBlueHats.TileNumber);
                        }
                        else if (name.Contains("Mask") && _board.hasBlueMasks.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasBlueMasks.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasBlueMasks.TileNumber);
                            newList.Add(_board.hasBlueMasks.TileNumber);
                        }
                        else if (name.Contains("Heel") && _board.hasBlueHeels.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasBlueHeels.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasBlueHeels.TileNumber);
                            newList.Add(_board.hasBlueHeels.TileNumber);
                        }
                        else if (name.Contains("Horseshoe") && _board.hasBlueHorseshoes.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasBlueHorseshoes.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasBlueHorseshoes.TileNumber);
                            newList.Add(_board.hasBlueHorseshoes.TileNumber);
                        }
                        else if (name.Contains("Gun") && _board.hasBlueGuns.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasBlueGuns.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasBlueGuns.TileNumber);
                            newList.Add(_board.hasBlueGuns.TileNumber);
                        }
                        else
                        {
                            for (int i = 0; i <= _wagons.Length; i++)
                            {
                                if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].BlueNumber == 0)
                                {
                                    TargetSetImageMaterial(client, i, MaterialColor.GREEN);
                                    TargetAddGreenOutline(client, i);
                                    newList.Add(i);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i <= _wagons.Length; i++)
                        {
                            if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].BlueNumber == 0)
                            {
                                print("added outline");
                                TargetSetImageMaterial(client, i, MaterialColor.GREEN);
                                TargetAddGreenOutline(client, i);
                                newList.Add(i);
                                break;
                            }
                        }
                    }
                }

                //_outlinedGreen = new List<int>(newList);
                break;
            case PlayerColor.RED:
                {
                    if (_board.Red > 0)
                    {
                        if (name.Contains("Hat") && _board.hasRedHats.Has)
                        {
                            //SetImageMaterial(_board.hasRedHats.TileNumber, MaterialColor.GREEN);
                            //_outlinedGreen.Add(_board.hasRedHats.TileNumber);

                            TargetSetImageMaterial(client, _board.hasRedHats.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasRedHats.TileNumber);
                            newList.Add(_board.hasRedHats.TileNumber);
                        }
                        else if (name.Contains("Mask") && _board.hasRedMasks.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasRedMasks.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasRedMasks.TileNumber);
                            newList.Add(_board.hasRedMasks.TileNumber);
                        }
                        else if (name.Contains("Heel") && _board.hasRedHeels.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasRedHeels.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasRedHeels.TileNumber);
                            newList.Add(_board.hasRedHeels.TileNumber);
                        }
                        else if (name.Contains("Horseshoe") && _board.hasRedHorseshoes.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasRedHorseshoes.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasRedHorseshoes.TileNumber);
                            newList.Add(_board.hasRedHorseshoes.TileNumber);
                        }
                        else if (name.Contains("Gun") && _board.hasRedGuns.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasRedGuns.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasRedGuns.TileNumber);
                            newList.Add(_board.hasRedGuns.TileNumber);
                        }
                        else
                        {
                            for (int i = 0; i <= _wagons.Length; i++)
                            {
                                if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].RedNumber == 0)
                                {
                                    TargetSetImageMaterial(client, i, MaterialColor.GREEN);
                                    TargetAddGreenOutline(client, i);
                                    newList.Add(i);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i <= _wagons.Length; i++)
                        {
                            if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].RedNumber == 0)
                            {
                                TargetSetImageMaterial(client, i, MaterialColor.GREEN);
                                TargetAddGreenOutline(client, i);
                                newList.Add(i);
                                break;
                            }
                        }
                    }
                }

                _outlinedGreen = new List<int>(newList);
                break;
            case PlayerColor.GREEN:
                {
                    if (_board.Green > 0)
                    {
                        if (name.Contains("Hat") && _board.hasGreenHats.Has)
                        {
                            //SetImageMaterial(_board.hasGreenHats.TileNumber, MaterialColor.GREEN);
                            //_outlinedGreen.Add(_board.hasGreenHats.TileNumber);

                            TargetSetImageMaterial(client, _board.hasGreenHats.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasGreenHats.TileNumber);
                            newList.Add(_board.hasGreenHats.TileNumber);
                        }
                        else if (name.Contains("Mask") && _board.hasGreenMasks.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasGreenMasks.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasGreenMasks.TileNumber);
                            newList.Add(_board.hasGreenMasks.TileNumber);
                        }
                        else if (name.Contains("Heel") && _board.hasGreenHeels.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasGreenHeels.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasGreenHeels.TileNumber);
                            newList.Add(_board.hasGreenHeels.TileNumber);
                        }
                        else if (name.Contains("Horseshoe") && _board.hasGreenHorseshoes.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasGreenHorseshoes.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasGreenHorseshoes.TileNumber);
                            newList.Add(_board.hasGreenHorseshoes.TileNumber);
                        }
                        else if (name.Contains("Gun") && _board.hasGreenGuns.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasGreenGuns.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasGreenGuns.TileNumber);
                            newList.Add(_board.hasGreenGuns.TileNumber);
                        }
                        else
                        {
                            for (int i = 0; i <= _wagons.Length; i++)
                            {
                                if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].GreenNumber == 0)
                                {
                                    TargetSetImageMaterial(client, i, MaterialColor.GREEN);
                                    TargetAddGreenOutline(client, i);
                                    newList.Add(i);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i <= _wagons.Length; i++)
                        {
                            if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].GreenNumber == 0)
                            {
                                TargetSetImageMaterial(client, i, MaterialColor.GREEN);
                                TargetAddGreenOutline(client, i);
                                newList.Add(i);
                                break;
                            }
                        }
                    }
                }

                _outlinedGreen = new List<int>(newList);
                break;
            case PlayerColor.YELLOW:
                {
                    if (_board.Yellow > 0)
                    {
                        if (name.Contains("Hat") && _board.hasYellowHats.Has)
                        {
                            //SetImageMaterial(_board.hasYellowHats.TileNumber, MaterialColor.GREEN);
                            //_outlinedGreen.Add(_board.hasYellowHats.TileNumber);

                            TargetSetImageMaterial(client, _board.hasYellowHats.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasYellowHats.TileNumber);
                            newList.Add(_board.hasYellowHats.TileNumber);
                        }
                        else if (name.Contains("Mask") && _board.hasYellowMasks.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasYellowMasks.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasYellowMasks.TileNumber);
                            newList.Add(_board.hasYellowMasks.TileNumber);
                        }
                        else if (name.Contains("Heel") && _board.hasYellowHeels.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasYellowHeels.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasYellowHeels.TileNumber);
                            newList.Add(_board.hasYellowHeels.TileNumber);
                        }
                        else if (name.Contains("Horseshoe") && _board.hasYellowHorseshoes.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasYellowHorseshoes.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasYellowHorseshoes.TileNumber);
                            newList.Add(_board.hasYellowHorseshoes.TileNumber);
                        }
                        else if (name.Contains("Gun") && _board.hasYellowGuns.Has)
                        {
                            TargetSetImageMaterial(client, _board.hasYellowGuns.TileNumber, MaterialColor.GREEN);
                            TargetAddGreenOutline(client, _board.hasYellowGuns.TileNumber);
                            newList.Add(_board.hasYellowGuns.TileNumber);
                        }
                        else
                        {
                            for (int i = 0; i <= _wagons.Length; i++)
                            {
                                if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].YellowNumber == 0)
                                {
                                    TargetSetImageMaterial(client, i, MaterialColor.GREEN);
                                    TargetAddGreenOutline(client, i);
                                    newList.Add(i);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i <= _wagons.Length; i++)
                        {
                            if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].YellowNumber == 0)
                            {
                                TargetSetImageMaterial(client, i, MaterialColor.GREEN);
                                TargetAddGreenOutline(client, i);
                                newList.Add(i);
                                break;
                            }
                        }
                    }
                }

                _outlinedGreen = new List<int>(newList);
                break;
        }
    }

    [TargetRpc]
    public void TargetAddGreenOutline(NetworkConnectionToClient conn, int index)
    {
        _outlinedGreen.Add(index);
    }

    [TargetRpc]
    public void TargetOutlineTilesWithDice(NetworkConnectionToClient client)
    {
        OutlineTilesWithDice();
    }

    public void OutlineTilesWithDice()
    {
        // Clear previous red/green outlines
        RemoveRedOutlines();
        RemoveGreenOutlines();
        _outlinedGreen.Clear();

        Debug.Log("OutlineTilesWithDice");
        // Outline green tiles
        var newList = new List<int>();
        for (int i = 0; i < _wagons.Length; i++)
        {
            if ((_wagons[i] != null) && _wagons[i].HasDice())
            {
                newList.Add(i);
                _outlinedGreen.Add(i);
                SetImageMaterial(i, MaterialColor.GREEN);
            }
        }
        //_outlinedGreen = new List<int>(newList);
    }

    public void RemoveGreenOutlines()
    {
        foreach (int i in _outlinedGreen)
        {
            SetImageMaterial(i, MaterialColor.NONE);
        }
        _outlinedGreen.Clear();
    }

    [TargetRpc]
    public void TargetRemoveGreenOutlines(NetworkConnectionToClient client)
    {
        RemoveGreenOutlines();
    }

    [TargetRpc]
    public void TargetRemoveRedOutlines(NetworkConnectionToClient conn)
    {
        RemoveRedOutlines();
    }

    [TargetRpc]
    public void TargetRemoveGreenOutline(NetworkConnectionToClient conn, int index)
    {
        SetImageMaterial(index, MaterialColor.NONE);
        RemoveGreenTile(index);
    }

    public void RemoveRedOutlines()
    {
        foreach (int i in _outlinedRed)
        {
            SetImageMaterial(i, MaterialColor.NONE);
        }
        _outlinedRed.Clear();
    }

    public void RemoveRedTile(int index)
    {
        _outlinedRed.Remove(index);
    }

    [TargetRpc]
    public void TargetRemoveRedTile(NetworkConnectionToClient conn, int index)
    {
        Debug.Log("TargetRemoveRedTile");
        _outlinedRed.Remove(index);
    }

    public void RemoveGreenTile(int index)
    {
        _outlinedGreen.Remove(index);
    }

    [Server]
    public void CheckIfTakesTrunk(NetworkConnectionToClient client, int index)
    {
        if (index >= 16) return;

        var canTakeTrunk = _wagons[index].CanTakeTrunk();
        Debug.Log("CheckIfTakesTrunk _trunkImages[index]._trunkBack.name: " + _trunkBacks[index].name);
        Debug.Log("canTakeTrunk: " + canTakeTrunk);
        var isNameEqual = _wagons[index].GetCurrentPlayerDiceName().Contains(_trunkBacks[index].name, StringComparison.OrdinalIgnoreCase);
        Debug.Log("name of dice on tile: " + _wagons[index].GetCurrentPlayerDiceName());
        Debug.Log("isNameEqual: " + isNameEqual);
        if (!_trunkTaken[index] && canTakeTrunk && isNameEqual)
        {
            Debug.Log("true");
            _rotateScripts[index].TakeTrunk(client);
            //_rotateScripts[index].enabled = true;
            _trunkTaken[index] = true;
        }
    }

    [Server]
    public bool IsTrunkTaken(int index)
    {
        return _trunkTaken[index];
    }

    [Server]
    public bool AreAllWagonsTaken()
    {
        for (int i = 0; i < 16; i++)
        {
            if (_wagons[i].isActiveAndEnabled) return false;
        }

        return true;
    }

    public void AddPointsForTrunk(string name)
    {
        int index = int.Parse(name[^1].ToString());

        //_pointCounterScript.AddTrunkPoints(_trunkImages[index - 1].points);
    }

    public void IncreaseTrunks()
    {
        _pointCounterScript.AddTrunk();
    }

    [Server]
    public void SwitchTileDice(NetworkConnectionToClient client, int firstTile, int secondTile)
    {
        Debug.Log("SwitchTileDice");
        Vector3 dicePos1;
        GameObject firstGroup;
        GameObject secondGroup;
        int number;
        switch (_turnManager.Current)
        {
            case PlayerColor.BLUE:
                dicePos1 = _wagons[firstTile].BlueGroup.transform.position;
                firstGroup = _wagons[firstTile].BlueGroup;
                number = _wagons[firstTile].BlueNumber;
                secondGroup = _wagons[secondTile].BlueGroup;

                SwitchDicePositions(firstGroup, secondGroup);

                //_wagons[indexes[0]].BlueGroup.transform.position = _wagons[indexes[1]].BlueGroup.transform.position;
                _wagons[firstTile].BlueGroup = _wagons[secondTile].BlueGroup;
                _wagons[firstTile].BlueNumber = _wagons[secondTile].BlueNumber;

                //_wagons[indexes[1]].BlueGroup.transform.position = dicePos1;
                _wagons[secondTile].BlueGroup = firstGroup;
                _wagons[secondTile].BlueNumber = number;
                break;
            case PlayerColor.RED:
                Debug.Log("Case RED");
                dicePos1 = _wagons[firstTile].RedGroup.transform.position;
                firstGroup = _wagons[firstTile].RedGroup;
                number = _wagons[firstTile].RedNumber;
                Debug.Log("index 0 passed");
                secondGroup = _wagons[secondTile].RedGroup;

                Debug.Log("hi");
                SwitchDicePositions(firstGroup, secondGroup);

                _wagons[firstTile].RedGroup = _wagons[secondTile].RedGroup;
                _wagons[firstTile].RedNumber = _wagons[secondTile].RedNumber;

                _wagons[secondTile].RedGroup = firstGroup;
                _wagons[secondTile].RedNumber = number;
                break;
            case PlayerColor.GREEN:
                dicePos1 = _wagons[firstTile].GreenGroup.transform.position;
                firstGroup = _wagons[firstTile].GreenGroup;
                number = _wagons[firstTile].GreenNumber;
                secondGroup = _wagons[secondTile].GreenGroup;

                SwitchDicePositions(firstGroup, secondGroup);

                _wagons[firstTile].GreenGroup = _wagons[secondTile].GreenGroup;
                _wagons[firstTile].GreenNumber = _wagons[secondTile].GreenNumber;

                _wagons[secondTile].GreenGroup = firstGroup;
                _wagons[secondTile].GreenNumber = number;
                break;
            case PlayerColor.YELLOW:
                dicePos1 = _wagons[firstTile].YellowGroup.transform.position;
                firstGroup = _wagons[firstTile].YellowGroup;
                number = _wagons[firstTile].YellowNumber;
                secondGroup = _wagons[secondTile].YellowGroup;

                SwitchDicePositions(firstGroup, secondGroup);

                _wagons[firstTile].YellowGroup = _wagons[secondTile].YellowGroup;
                _wagons[firstTile].YellowNumber = _wagons[secondTile].YellowNumber;

                _wagons[secondTile].YellowGroup = firstGroup;
                _wagons[secondTile].YellowNumber = number;
                break;
        }

        //_trunkPlacerScript.DisableUsedTrunk();

        CheckIfTakesTrunk(client, firstTile);
        CheckIfTakesTrunk(client, secondTile);

        Debug.Log("hehehe");
        TargetDestroyTrunk(client);
        //_trunkToDestroy.SetActive(false);
        Debug.Log("hehehe");
        //Destroy(_trunkToDestroy);
        TargetRemoveGreenOutlines(client);
        Debug.Log("hehehe");
    }

    [TargetRpc]
    private void TargetDestroyTrunk(NetworkConnectionToClient client)
    {
        Debug.Log("TargetDestroyTrunk");
        _trunkPlacerScript.DisableUsedTrunk();
        if (_trunkToDestroy != null)
        {
            _trunkToDestroy.SetActive(false);
            Debug.Log("trunk inactive");
            //Destroy(_trunkToDestroy);
            Debug.Log("trunk destroyed");
        }
    }

    private void SwitchDicePositions(GameObject firstGroup, GameObject secondGroup)
    {
        Debug.Log("SwitchDicePositions");
        List<Rigidbody> firstChildRbs = new List<Rigidbody>();
        foreach (Transform child in firstGroup.transform)
        {
            firstChildRbs.Add(child.GetComponent<Rigidbody>());
        }

        List<Rigidbody> secondChildRbs = new List<Rigidbody>();
        foreach (Transform child in secondGroup.transform)
        {
            secondChildRbs.Add(child.GetComponent<Rigidbody>());
        }

        Debug.Log("firstChildRbs.Count: " +  firstChildRbs.Count);
        Debug.Log("secondChildRbs.Count: " +  secondChildRbs.Count);

        var pos1 = new Vector3(firstChildRbs[0].transform.position.x, firstChildRbs[0].transform.position.y, firstChildRbs[0].transform.position.z);
        var pos2 = new Vector3(secondChildRbs[0].transform.position.x, secondChildRbs[0].transform.position.y, secondChildRbs[0].transform.position.z);

        firstChildRbs[0].transform.position = (new Vector3(pos2.x, pos2.y + 1f, pos2.z));
        secondChildRbs[0].transform.position = (new Vector3(pos1.x, pos1.y + 1f, pos1.z));

        for (int i = 1; i < firstChildRbs.Count; i++)
        {
            firstChildRbs[i].transform.position = (new Vector3(pos2.x + i, pos2.y + 1f, pos2.z));
            Debug.Log("i: " + i);
        }

        for (int i = 1; i < secondChildRbs.Count; i++)
        {
            secondChildRbs[i].transform.position = (new Vector3(pos1.x + i, pos1.y + 1f, pos1.z));
            Debug.Log("i: " + i);
        }
    }

    public bool AtLeastTwoTilesWithDice()
    {
        int counter = 0;

        foreach (var imgProp in _wagons)
        {
            if (imgProp.HasDice())
            {
                counter++;
                if (counter == 2)
                    return true;
            }
        }

        return false;
    }

    public void ClearBlueGroup(string groupName)
    {
        if (groupName.Contains("Hat"))
        {
            _board.hasBlueHats.Has = false;
        }
        else if (groupName.Contains("Gun"))
        {
            _board.hasBlueGuns.Has = false;
        }
        else if (groupName.Contains("Horseshoe"))
        {
            _board.hasBlueHorseshoes.Has = false;
        }
        else if (groupName.Contains("Heel"))
        {
            _board.hasBlueHeels.Has = false;
        }
        else if (groupName.Contains("Mask"))
        {
            _board.hasBlueMasks.Has = false;
        }
    }

    public void ClearRedGroup(string groupName)
    {
        if (groupName.Contains("Hat"))
        {
            _board.hasRedHats.Has = false;
        }
        else if (groupName.Contains("Gun"))
        {
            _board.hasRedGuns.Has = false;
        }
        else if (groupName.Contains("Horseshoe"))
        {
            _board.hasRedHorseshoes.Has = false;
        }
        else if (groupName.Contains("Heel"))
        {
            _board.hasRedHeels.Has = false;
        }
        else if (groupName.Contains("Mask"))
        {
            _board.hasRedMasks.Has = false;
        }
    }

    public void ClearGreenGroup(string groupName)
    {
        if (groupName.Contains("Hat"))
        {
            _board.hasGreenHats.Has = false;
        }
        else if (groupName.Contains("Gun"))
        {
            _board.hasGreenGuns.Has = false;
        }
        else if (groupName.Contains("Horseshoe"))
        {
            _board.hasGreenHorseshoes.Has = false;
        }
        else if (groupName.Contains("Heel"))
        {
            _board.hasGreenHeels.Has = false;
        }
        else if (groupName.Contains("Mask"))
        {
            _board.hasGreenMasks.Has = false;
        }
    }

    public void ClearYellowGroup(string groupName)
    {
        if (groupName.Contains("Hat"))
        {
            _board.hasYellowHats.Has = false;
        }
        else if (groupName.Contains("Gun"))
        {
            _board.hasYellowGuns.Has = false;
        }
        else if (groupName.Contains("Horseshoe"))
        {
            _board.hasYellowHorseshoes.Has = false;
        }
        else if (groupName.Contains("Heel"))
        {
            _board.hasYellowHeels.Has = false;
        }
        else if (groupName.Contains("Mask"))
        {
            _board.hasYellowMasks.Has = false;
        }
    }

    [ClientRpc]
    public void RpcClearBlueGroup(string groupName)
    {
        ClearBlueGroup(groupName);
    }

    [ClientRpc]
    public void RpcClearRedGroup(string groupName)
    {
        ClearRedGroup(groupName);
    }

    [ClientRpc]
    public void RpcClearGreenGroup(string groupName)
    {
        ClearGreenGroup(groupName);
    }

    [ClientRpc]
    public void RpcClearYellowGroup(string groupName)
    {
        ClearYellowGroup(groupName);
    }








    public void OutlineAvailableTile(string name)
    {
        // Clear previous red/green outlines
        RemoveRedOutlines();
        RemoveGreenOutlines();
        _outlinedGreen.Clear();

        PlayerColor currentPlayer = _turnManager.Current;

        var newList = new List<int>();
        switch (currentPlayer)
        {
            case PlayerColor.BLUE:
                {
                    Debug.Log("_board.Blue = " + _board.Blue);
                    if (_board.Blue > 0)
                    {
                        if (name.Contains("Hat") && _board.hasBlueHats.Has)
                        {
                            SetImageMaterial(_board.hasBlueHats.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasBlueHats.TileNumber);
                            newList.Add(_board.hasBlueHats.TileNumber);
                        }
                        else if (name.Contains("Mask") && _board.hasBlueMasks.Has)
                        {
                            SetImageMaterial(_board.hasBlueMasks.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasBlueMasks.TileNumber);
                            newList.Add(_board.hasBlueMasks.TileNumber);
                        }
                        else if (name.Contains("Heel") && _board.hasBlueHeels.Has)
                        {
                            SetImageMaterial(_board.hasBlueHeels.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasBlueHeels.TileNumber);
                            newList.Add(_board.hasBlueHeels.TileNumber);
                        }
                        else if (name.Contains("Horseshoe") && _board.hasBlueHorseshoes.Has)
                        {
                            SetImageMaterial(_board.hasBlueHorseshoes.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasBlueHorseshoes.TileNumber);
                            newList.Add(_board.hasBlueHorseshoes.TileNumber);
                        }
                        else if (name.Contains("Gun") && _board.hasBlueGuns.Has)
                        {
                            SetImageMaterial(_board.hasBlueGuns.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasBlueGuns.TileNumber);
                            newList.Add(_board.hasBlueGuns.TileNumber);
                        }
                        else
                        {
                            for (int i = 0; i < _wagons.Length; i++)
                            {
                                if (_wagons[i] != null)
                                {
                                    Debug.Log("_wagons[i] != null");
                                }
                                if (_wagons[i].isActiveAndEnabled)
                                {
                                    Debug.Log("_wagons[i].isActiveAndEnabled");
                                }
                                if (_wagons[i].BlueNumber == 0)
                                {
                                    Debug.Log("_wagons[i].BlueNumber == 0");
                                }
                                if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].BlueNumber == 0)
                                {
                                    SetImageMaterial(i, MaterialColor.GREEN);
                                    _outlinedGreen.Add(i);
                                    newList.Add(i);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _wagons.Length; i++)
                        {
                            if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].BlueNumber == 0)
                            {
                                print("added outline");
                                SetImageMaterial(i, MaterialColor.GREEN);
                                _outlinedGreen.Add(i);
                                newList.Add(i);
                                break;
                            }
                        }
                    }
                }

                //_outlinedGreen = new List<int>(newList);
                break;
            case PlayerColor.RED:
                {
                    if (_board.Red > 0)
                    {
                        Debug.Log("_board.Red > 0");
                        Debug.Log("_board.hasRedHats: " + _board.hasRedHats.Has + ", " + _board.hasRedHats.TileNumber);
                        if (name.Contains("Hat") && _board.hasRedHats.Has)
                        {
                            Debug.Log("hat");
                            //SetImageMaterial(_board.hasRedHats.TileNumber, MaterialColor.GREEN);
                            //_outlinedGreen.Add(_board.hasRedHats.TileNumber);

                            SetImageMaterial(_board.hasRedHats.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasRedHats.TileNumber);
                            newList.Add(_board.hasRedHats.TileNumber);
                        }
                        else if (name.Contains("Mask") && _board.hasRedMasks.Has)
                        {
                            Debug.Log("mask");
                            SetImageMaterial(_board.hasRedMasks.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasRedMasks.TileNumber);
                            newList.Add(_board.hasRedMasks.TileNumber);
                        }
                        else if (name.Contains("Heel") && _board.hasRedHeels.Has)
                        {
                            Debug.Log("heel");
                            SetImageMaterial(_board.hasRedHeels.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasRedHeels.TileNumber);
                            newList.Add(_board.hasRedHeels.TileNumber);
                        }
                        else if (name.Contains("Horseshoe") && _board.hasRedHorseshoes.Has)
                        {
                            Debug.Log("horseshoe");
                            SetImageMaterial(_board.hasRedHorseshoes.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasRedHorseshoes.TileNumber);
                            newList.Add(_board.hasRedHorseshoes.TileNumber);
                        }
                        else if (name.Contains("Gun") && _board.hasRedGuns.Has)
                        {
                            Debug.Log("gun");
                            SetImageMaterial(_board.hasRedGuns.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasRedGuns.TileNumber);
                            newList.Add(_board.hasRedGuns.TileNumber);
                        }
                        else
                        {
                            for (int i = 0; i < _wagons.Length; i++)
                            {
                                if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].RedNumber == 0)
                                {
                                    SetImageMaterial(i, MaterialColor.GREEN);
                                    _outlinedGreen.Add(i);
                                    newList.Add(i);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _wagons.Length; i++)
                        {
                            if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].RedNumber == 0)
                            {
                                SetImageMaterial(i, MaterialColor.GREEN);
                                _outlinedGreen.Add(i);
                                newList.Add(i);
                                break;
                            }
                        }
                    }
                }

                //_outlinedGreen = new List<int>(newList);
                break;
            case PlayerColor.GREEN:
                {
                    if (_board.Green > 0)
                    {
                        if (name.Contains("Hat") && _board.hasGreenHats.Has)
                        {
                            //SetImageMaterial(_board.hasGreenHats.TileNumber, MaterialColor.GREEN);
                            //_outlinedGreen.Add(_board.hasGreenHats.TileNumber);

                            SetImageMaterial(_board.hasGreenHats.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasGreenHats.TileNumber);
                            newList.Add(_board.hasGreenHats.TileNumber);
                        }
                        else if (name.Contains("Mask") && _board.hasGreenMasks.Has)
                        {
                            SetImageMaterial(_board.hasGreenMasks.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasGreenMasks.TileNumber);
                            newList.Add(_board.hasGreenMasks.TileNumber);
                        }
                        else if (name.Contains("Heel") && _board.hasGreenHeels.Has)
                        {
                            SetImageMaterial(_board.hasGreenHeels.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasGreenHeels.TileNumber);
                            newList.Add(_board.hasGreenHeels.TileNumber);
                        }
                        else if (name.Contains("Horseshoe") && _board.hasGreenHorseshoes.Has)
                        {
                            SetImageMaterial(_board.hasGreenHorseshoes.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasGreenHorseshoes.TileNumber);
                            newList.Add(_board.hasGreenHorseshoes.TileNumber);
                        }
                        else if (name.Contains("Gun") && _board.hasGreenGuns.Has)
                        {
                            SetImageMaterial(_board.hasGreenGuns.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasGreenGuns.TileNumber);
                            newList.Add(_board.hasGreenGuns.TileNumber);
                        }
                        else
                        {
                            for (int i = 0; i < _wagons.Length; i++)
                            {
                                if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].GreenNumber == 0)
                                {
                                    SetImageMaterial(i, MaterialColor.GREEN);
                                    _outlinedGreen.Add(i);
                                    newList.Add(i);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _wagons.Length; i++)
                        {
                            if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].GreenNumber == 0)
                            {
                                SetImageMaterial(i, MaterialColor.GREEN);
                                _outlinedGreen.Add(i);
                                newList.Add(i);
                                break;
                            }
                        }
                    }
                }

                //_outlinedGreen = new List<int>(newList);
                break;
            case PlayerColor.YELLOW:
                {
                    if (_board.Yellow > 0)
                    {
                        if (name.Contains("Hat") && _board.hasYellowHats.Has)
                        {
                            //SetImageMaterial(_board.hasYellowHats.TileNumber, MaterialColor.GREEN);
                            //_outlinedGreen.Add(_board.hasYellowHats.TileNumber);

                            SetImageMaterial(_board.hasYellowHats.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasYellowHats.TileNumber);
                            newList.Add(_board.hasYellowHats.TileNumber);
                        }
                        else if (name.Contains("Mask") && _board.hasYellowMasks.Has)
                        {
                            SetImageMaterial(_board.hasYellowMasks.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasYellowMasks.TileNumber);
                            newList.Add(_board.hasYellowMasks.TileNumber);
                        }
                        else if (name.Contains("Heel") && _board.hasYellowHeels.Has)
                        {
                            SetImageMaterial(_board.hasYellowHeels.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasYellowHeels.TileNumber);
                            newList.Add(_board.hasYellowHeels.TileNumber);
                        }
                        else if (name.Contains("Horseshoe") && _board.hasYellowHorseshoes.Has)
                        {
                            SetImageMaterial(_board.hasYellowHorseshoes.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasYellowHorseshoes.TileNumber);
                            newList.Add(_board.hasYellowHorseshoes.TileNumber);
                        }
                        else if (name.Contains("Gun") && _board.hasYellowGuns.Has)
                        {
                            SetImageMaterial(_board.hasYellowGuns.TileNumber, MaterialColor.GREEN);
                            _outlinedGreen.Add(_board.hasYellowGuns.TileNumber);
                            newList.Add(_board.hasYellowGuns.TileNumber);
                        }
                        else
                        {
                            for (int i = 0; i < _wagons.Length; i++)
                            {
                                if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].YellowNumber == 0)
                                {
                                    SetImageMaterial(i, MaterialColor.GREEN);
                                    _outlinedGreen.Add(i);
                                    newList.Add(i);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _wagons.Length; i++)
                        {
                            if (_wagons[i] != null && _wagons[i].isActiveAndEnabled && _wagons[i].YellowNumber == 0)
                            {
                                SetImageMaterial(i, MaterialColor.GREEN);
                                _outlinedGreen.Add(i);
                                newList.Add(i);
                                break;
                            }
                        }
                    }
                }

                //_outlinedGreen = new List<int>(newList);
                break;
        }
    }
}
