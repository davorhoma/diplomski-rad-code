using Assets.Scripts;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

//public enum Action { ATTACK = 0, STEAL = 1, RETREAT = 2, ROLL = 3, AMBUSH = 4 };
public class ClickTileEvent : NetworkBehaviour
{
    [SyncVar] private PlayerAction action;
    [SerializeField] GameObject[] _tiles;
    [SerializeField] RectTransform[] _rectTransforms;
    [SerializeField] SelectOutline _selectOutlineScript;
    [SerializeField] PointCounter _pointCounterScript;
    [SerializeField] private EndGameMenu _endGameMenuScript;
    private TurnManager _turnManager;
    private GameObject _selectedDiceGroup;
    //private float x = 0f;
    [SerializeField, FormerlySerializedAs("_imageProperties")] Wagon[] _wagons;
    [SerializeField] GameObject[] _trunks;
    [SerializeField] ImageAssigner _imageAssigner;
    [SerializeField, FormerlySerializedAs("_boardProperties")] Board _board;

    private string availableTile = "AvailableTile";
    private string untaggedTile = "Untagged";
    private string tileWithDice = "TileWithDice";

    private bool _appendDice;
    private string _groupName;
    public string GroupName { get { return _groupName; } set { _groupName = value; } }
    public bool AppendDice {  get { return _appendDice; } set {  _appendDice = value; } }

    [SerializeField, FormerlySerializedAs("_diceManager")] private DiceSetupManager _diceSetupManager;
    [SerializeField] private Retreat _retreatScript;
    private PlayerCommandController _playerCommandController;

    private void Start()
    {
        _turnManager = GetComponent<TurnManager>();
        _playerCommandController = NetworkClient.localPlayer.GetComponent<PlayerCommandController>();

        AddEventByScript(_tiles);
    }

    private void Update()
    {
        if (_selectOutlineScript.SelectedDiceGroup != null)
        {
            _selectedDiceGroup = _selectOutlineScript.SelectedDiceGroup;
        }
    }

    private IEnumerator SetTransformParent(Transform parent)
    {
        float childCount = parent.childCount;
        var firstChildPos = parent.GetChild(0).position;

        float a = 0f;
        Debug.Log("_selectedDiceGroup.transform.childCount = " + _selectedDiceGroup.transform.childCount);
        Debug.Log("childCount: " + childCount);

        List<Transform> children = new List<Transform>();
        foreach (Transform transform in _selectedDiceGroup.transform)
        {
            var rb = transform.GetComponent<Rigidbody>();
            Debug.Log("Moving die on a tile");
            transform.GetComponent<Rigidbody>().MovePosition(new Vector3(firstChildPos.x + childCount + a, firstChildPos.y + 1f, firstChildPos.z));
            a += 1f;
            children.Add(transform);
            //transform.position = lastDie.position + new Vector3(0.45f, 100, 10);
        }

        yield return new WaitForSeconds(0.1f);

        Debug.Log("_selectedDiceGroup.transform.childCount = " + _selectedDiceGroup.transform.childCount);
        foreach (var child in children)
        {
            child.SetParent(parent, true);
            _turnManager.SetTagPositioned(child.gameObject);    

            _turnManager.RpcSetTagPositioned(child.GetComponent<NetworkIdentity>().netId);
            _diceSetupManager.RpcSetParentWithNetId(child.GetComponent<NetworkIdentity>().netId, parent.GetComponent<NetworkIdentity>().netId);
        }

    }

    private void AttackWagon(int index)
    {
        Debug.Log("We click on the image");
        if (_selectedDiceGroup == null || _selectedDiceGroup.tag.Contains("Positioned") || _selectOutlineScript.WasSelected == false)
        {
            if (_selectedDiceGroup == null) Debug.Log("Null");
            if (_selectedDiceGroup.tag.Contains("Positioned")) Debug.Log("Positioned");
            if (_selectOutlineScript.WasSelected == false) Debug.Log("WasSelected");
            Debug.Log("You need to select a \"selectable\" die (dice).");
            return;
        }

        if (!_imageAssigner.IsOutlined(index))
        {
            Debug.Log("!_imageAssigner.IsOutlined(index)");
            return;
        }

        Debug.Log("passde");
        var script = NetworkClient.localPlayer.GetComponent<PlayerCommandController>();
        if (script == null)
        {
            Debug.Log("Script == null");
        }

        Debug.Log("pasdsada");
        script.CmdAttack(index, _selectedDiceGroup.GetComponent<NetworkIdentity>().netId, _groupName);
    }

    [Server]
    public void ServerAttackWagon(int index, uint selectedGroupNetId, string groupName, NetworkConnectionToClient client)
    {
        if (!NetworkClient.spawned.TryGetValue(selectedGroupNetId, out var networkIdentity))
        {
            Debug.Log("Cannot find selected group by netId");
            return;
        }
        _selectedDiceGroup = networkIdentity.gameObject;
        int groupIndex = groupName.IndexOf("Group"); // Find "Group" in the string
        _groupName = groupIndex > 0 ? groupName.Substring(0, groupIndex) : groupName;
        Debug.Log("_groupName: " + _groupName);

        Debug.Log("ServerAttackWagon");
        GameObject waypoint;
        if (index == 16)
            waypoint = _tiles[16];
        else
            waypoint = _tiles[index];

        Image targetImage = waypoint.GetComponent<Image>();

        PlayerColor currentPlayer = _turnManager.Current;
        if (currentPlayer == PlayerColor.BLUE)
        {
            StartCoroutine(AttackBlue(client, index, waypoint, 0.1f));
        }
        else if (currentPlayer == PlayerColor.RED)
        {
            StartCoroutine(AttackRed(client, index, waypoint, 0.1f));
        }
        else if (currentPlayer == PlayerColor.GREEN)
        {
            StartCoroutine(AttackGreen(client, index, waypoint, 0.1f));
        }
        else
        {
            StartCoroutine(AttackYellow(client, index, waypoint, 0.1f));
        }
    }

    private void StealWagon(int index)
    {
        if (!_imageAssigner.OutlinedRed.Contains(index))
        {
            Debug.Log("You cannot steal this wagon/locomotive.");
            return;
        }

        var script = NetworkClient.localPlayer.GetComponent<PlayerCommandController>();
        if (script == null)
        {
            Debug.Log("Script == null");
            return;
        }

        script.CmdSteal(index);

        //PlayerColor currentPlayer = _turnManager.Current;
        //if (currentPlayer == PlayerColor.BLUE)
        //{
        //    _pointCounterScript.AddWagonPoints(_wagons[index].AssignedNumber);

        //    if (index == 16 && _imageAssigner.AreAllWagonsTaken())
        //    {
        //        _endGameMenuScript.EndGame();
        //    }

        //    MoveDiceFromTile(index);
        //    DealWithWagons(index);
        //}
        //else if (currentPlayer == PlayerColor.RED)
        //{
        //    _pointCounterScript.AddWagonPoints(_wagons[index].AssignedNumber);

        //    if (index == 16 && _imageAssigner.AreAllWagonsTaken())
        //    {
        //        _endGameMenuScript.EndGame();
        //    }

        //    MoveDiceFromTile(index);
        //    DealWithWagons(index);
        //}
        //else if (currentPlayer == PlayerColor.GREEN)
        //{
        //    _pointCounterScript.AddWagonPoints(_wagons[index].AssignedNumber);

        //    if (index == 16 && _imageAssigner.AreAllWagonsTaken())
        //    {
        //        _endGameMenuScript.EndGame();
        //    }

        //    MoveDiceFromTile(index);
        //    DealWithWagons(index);
        //}
        //else
        //{
        //    _pointCounterScript.AddWagonPoints(_wagons[index].AssignedNumber);

        //    if (index == 16 && _imageAssigner.AreAllWagonsTaken())
        //    {
        //        _endGameMenuScript.EndGame();
        //    }

        //    MoveDiceFromTile(index);
        //    DealWithWagons(index);
        //}

        //if (index == 16)
        //{
        //    Debug.Log("End game");
        //    ShowWinnerScreen();
        //}
    }

    // Default value for parameter 'conn' is null in case this method is called during ATTACK phase.
    // During ATTACK phase, when player has 7 dice on a wagon, he steals it instantly without highlighting the tile.
    [Server]
    public void ServerStealWagon(int index, NetworkConnectionToClient conn = null)
    {
        PlayerColor currentPlayer = _turnManager.Current;
        if (currentPlayer == PlayerColor.BLUE)
        {
            _pointCounterScript.AddWagonPoints(_wagons[index].AssignedNumber);

            if (index == 16 && _imageAssigner.AreAllWagonsTaken())
            {
                _endGameMenuScript.EndGame();
            }

            MoveDiceFromTile(index);
            DealWithWagons(index, conn);
        }
        else if (currentPlayer == PlayerColor.RED)
        {
            _pointCounterScript.AddWagonPoints(_wagons[index].AssignedNumber);

            if (index == 16 && _imageAssigner.AreAllWagonsTaken())
            {
                _endGameMenuScript.EndGame();
            }

            MoveDiceFromTile(index);
            DealWithWagons(index, conn);
        }
        else if (currentPlayer == PlayerColor.GREEN)
        {
            _pointCounterScript.AddWagonPoints(_wagons[index].AssignedNumber);

            if (index == 16 && _imageAssigner.AreAllWagonsTaken())
            {
                _endGameMenuScript.EndGame();
            }

            MoveDiceFromTile(index);
            DealWithWagons(index, conn);
        }
        else
        {
            _pointCounterScript.AddWagonPoints(_wagons[index].AssignedNumber);

            if (index == 16 && _imageAssigner.AreAllWagonsTaken())
            {
                _endGameMenuScript.EndGame();
            }

            MoveDiceFromTile(index);
            DealWithWagons(index, conn);
        }

        if (index == 16)
        {
            Debug.Log("End game");
            TargetShowWinnerScreen(conn);
            //ShowWinnerScreen();
        }
    }

    private void ShowWinnerScreen()
    {
        SceneManager.LoadScene(Scenes.Congratulation);
    }

    [TargetRpc]
    private void TargetShowWinnerScreen(NetworkConnectionToClient conn)
    {
        //SceneManager.LoadScene(Scenes.Congratulation);
        StartCoroutine(LoadSceneAfterDelay(Scenes.Congratulation, 1.5f));
    }

    private IEnumerator LoadSceneAfterDelay(string scene, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(scene);
    }

    [Server]
    private void DealWithWagons(int index, NetworkConnectionToClient conn)
    {
        if (_wagons[index].Last)
        {
            _wagons[index].gameObject.SetActive(false);
            uint wagonToDeactivateId = _wagons[index].GetComponent<NetworkIdentity>().netId;
            RpcDisableGameObject(wagonToDeactivateId);

            if (index < _trunks.Length && !_imageAssigner.IsTrunkTaken(index))
            {
                _trunks[index].SetActive(false);

                uint objectToDeactivateId = _trunks[index].GetComponent<NetworkIdentity>().netId;
                RpcDisableGameObject(objectToDeactivateId);
            }

            // TODO: This needs to be done for the current player, not only on the server.
            // IMPORTANT: This needs to be done for the current player, not only on the server.
            if (conn != null) 
                _imageAssigner.TargetRemoveRedTile(conn, index);
            //_imageAssigner.RemoveRedTile(index);

            // Assign new last wagon
            for (int i = index + 1; i < _wagons.Length; i++)
            {
                if (_wagons[i].isActiveAndEnabled)
                {
                    _wagons[i].Last = true;
                    break;
                }
            }
        }
        else
        {
            ShiftWagons(index);
            _wagons[index].gameObject.SetActive(false);
            uint wagonToDeactivateId = _wagons[index].GetComponent<NetworkIdentity>().netId;
            RpcDisableGameObject(wagonToDeactivateId);

            if (index < 16 && _trunks[index] != null && !_imageAssigner.IsTrunkTaken(index))
            {
                _trunks[index].SetActive(false);

                uint trunkToDeactivateId = _trunks[index].GetComponent<NetworkIdentity>().netId;
                RpcDisableGameObject(trunkToDeactivateId);
            }
            // TODO: This needs to be done for the current player, not only on the server.
            // IMPORTANT: This needs to be done for the current player, not only on the server.
            if (conn != null) 
                _imageAssigner.TargetRemoveRedTile(conn, index);
            //_imageAssigner.RemoveRedTile(index);

            // if (_imageProperty.Last) onda treba samo staviti visible na false (ili BOLJE skroz ukloniti)
            // else treba pomeriti sve prethodne za jednu poziciju na blize i ovaj staviti visible na false (ili BOLJE skroz ukloniti)
            // skinuti sve kocke sa slike

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

    public IEnumerator AttackBlue(NetworkConnectionToClient client, int index, GameObject waypoint, float time = 0.1f)
    {
        yield return new WaitForSeconds(time);

        Image targetImage = waypoint.GetComponent<Image>();
        Debug.Log("AttackWagon Blue");
        if (_wagons[index].BlueGroup != null && _wagons[index].BlueGroup.name.Contains(_groupName) && _wagons[index].BlueNumber > 0)
        {
            Debug.Log("First if");
            Transform parent = _wagons[index].BlueGroup.transform;
            StartCoroutine(SetTransformParent(parent));

            print("GroupName: " + _groupName);

            //_wagons[index].BlueGroup = parent.gameObject;
            Debug.Log("_wagons[index].BlueNumber = " + _wagons[index].BlueNumber);
            Debug.Log("_wagons[index].BlueNumber += _selectedDiceGroup.transform.childCount = " + _selectedDiceGroup.transform.childCount);
            _wagons[index].BlueNumber += _selectedDiceGroup.transform.childCount;
            Debug.Log("New _wagons[index].BlueNumber = " + _wagons[index].BlueNumber);

            _board.SetBlueGroup(_selectedDiceGroup.name, true, index);

            _imageAssigner.CheckIfTakesTrunk(client, index);
        }
        else if (_selectedDiceGroup.name.Contains("JokerGroup") && waypoint.CompareTag(tileWithDice))
        {
            Debug.Log("Second if");
            if (_wagons[index].BlueNumber == 0)
            {
                print("You cannot place jokers on an empty wagon/locomotive");
            }
            else
            {
                Transform parent = _wagons[index].BlueGroup.transform;
                Transform placedDie = parent.GetChild(0);
                float x = placedDie.position.x;
                x += _wagons[index].BlueNumber * 1f;
                float y = placedDie.position.y;
                float z = placedDie.position.z;
                //Transform placedDie = _wagons[index].BlueGroup.transform.GetChild(0);
                var children = PositionJokers("BluePositioned", placedDie.eulerAngles, new Vector3(x, y + 1f, z));
                _wagons[index].BlueNumber += children.Count;

                //foreach (Transform child in _selectedDiceGroup.transform)
                //{
                //    children.Add(child);
                //    print("Placed die x: " + placedDie.eulerAngles.x);
                //    child.eulerAngles = new Vector3(placedDie.eulerAngles.x, placedDie.eulerAngles.y, placedDie.eulerAngles.z);
                //    print("Child euler angles: " + child.eulerAngles);
                //    child.position = new Vector3(x, y + 1f, z);
                //    x += 1f;
                //    _wagons[index].BlueNumber++;
                //    child.tag = "BluePositioned";

                //    if (i == _selectOutlineScript.JokerToPosition)
                //        break;
                //    i++;
                //}

                foreach (var child in children)
                {
                    child.SetParent(parent, true);
                    _diceSetupManager.RpcSetParentWithNetId(child.GetComponent<NetworkIdentity>().netId, parent.GetComponent<NetworkIdentity>().netId);
                }

                //_wagons[index].BlueNumber += _selectedDiceGroup.transform.childCount;
                _board.SetBlueGroup(_selectedDiceGroup.name, true, index);

                _imageAssigner.CheckIfTakesTrunk(client, index);
            }
        }
        else if (waypoint == null || !_imageAssigner.IsOutlined(index) || _selectedDiceGroup.name.Contains("JokerGroup"))
        {
            if (waypoint == null)
            {
                Debug.Log("waypoing == null");
            }
            if (!_imageAssigner.IsOutlined(index))
            {
                Debug.Log("!_imageAssigner.IsOutlined(index)");
            }
            if (_selectedDiceGroup.name.Contains("JokerGroup"))
            {
                Debug.Log("joker group");
            }
            Debug.Log("Third if");
            print("Waypoint is null or not available");

            yield break;
        }
        else
        {
            Debug.Log("Fourth if");
            MoveSelectedDiceToTile(targetImage.rectTransform.position, -1.5f);

            _wagons[index].BlueGroup = _selectedDiceGroup;
            print("Selected dice group name: " + _selectedDiceGroup.name);
            _wagons[index].BlueNumber += _selectedDiceGroup.transform.childCount;
            Debug.Log("_wagons[index].BlueGroup.name = " + _wagons[index].BlueGroup.name);

            _tiles[index].tag = untaggedTile;

            //_imageAssigner.SetImageMaterial(index, MaterialColor.NONE);
            //_imageAssigner.RemoveGreenTile(index);

            _board.Blue++;
            _board.SetBlueGroup(_selectedDiceGroup.name, true, index);

            waypoint.tag = tileWithDice;
            for (int i = index + 1; i < 16; i++)
            {
                if (!_wagons[i].HasDice())
                {
                    _wagons[i].tag = availableTile;
                    break;
                }
            }

            _imageAssigner.CheckIfTakesTrunk(client, index);
        }

        _imageAssigner.TargetRemoveGreenOutlines(client);

        // TODO: Decide what to do after this ? automatically end turn : allow player to end turn
        // When player has the ability to end turn, he currently can move the dice freely (bug).
        // Instantly steal a wagon when player has all dice on it
        if (_wagons[index].BlueNumber == 7)
        {
            yield return new WaitForSeconds(2f);
            ServerStealWagon(index);
        }
    }

    private List<Transform> PositionJokers(string tag, Vector3 eulerAngles, Vector3 position)
    {
        var children = new List<Transform>();
        int i = 0;
        Debug.Log("_selectOutlineScript.jokerToPosition: " + _selectOutlineScript.JokerToPosition);
        foreach (Transform child in _selectedDiceGroup.transform)
        {
            children.Add(child);
            print("Placed die x: " + eulerAngles.x);
            child.eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, eulerAngles.z);
            print("Child euler angles: " + child.eulerAngles);
            child.position = position;
            position.x += 1f;
            child.tag = tag;
            _turnManager.RpcSetTagPositioned(child.GetComponent<NetworkIdentity>().netId);

            i++;
            if (i == _selectOutlineScript.JokerToPosition)
                break;
        }

        return children;
    }

    private void MoveSelectedDiceToTile(Vector3 pos, float z)
    {
        pos.x -= 2.5f;
        pos.y = 1f;
        //pos.z -= 1.5f;
        pos.z += z;
        //_selectedDiceGroup.transform.position = pos;
        _turnManager.SetTagPositioned(_selectedDiceGroup);
        _turnManager.RpcSetTagPositioned(_selectedDiceGroup.GetComponent<NetworkIdentity>().netId);

        foreach (Transform child in _selectedDiceGroup.transform)
        {
            //string childTag = child.tag;
            Debug.Log("Moving 1 cube");
            child.GetComponent<Rigidbody>().MovePosition(pos);
            pos.x += 1f;

            _turnManager.SetTagPositioned(child.gameObject);
            _turnManager.RpcSetTagPositioned(child.GetComponent<NetworkIdentity>().netId);
            //child.tag = "Positioned";
        }
    }

    public IEnumerator AttackRed(NetworkConnectionToClient client, int index, GameObject waypoint, float time = 0.1f)
    {
        yield return new WaitForSeconds(time);
        Debug.Log("_groupName: " + _groupName);

        Image targetImage = waypoint.GetComponent<Image>();
        if (_wagons[index].RedGroup != null && _wagons[index].RedGroup.name.Contains(_groupName) && _wagons[index].RedNumber > 0)
        {
            Debug.Log("first if");
            Transform parent = _wagons[index].RedGroup.transform;
            StartCoroutine(SetTransformParent(parent));

            //_wagons[index].RedGroup = parent.gameObject;
            _wagons[index].RedNumber = _selectedDiceGroup.transform.childCount;

            _board.SetRedGroup(_selectedDiceGroup.name, true, index);

            _imageAssigner.CheckIfTakesTrunk(client, index);
        }
        else if (_selectedDiceGroup.name.Contains("JokerGroup") && waypoint.CompareTag(tileWithDice))
        {
            Debug.Log("second if");
            if (_wagons[index].RedNumber == 0)
            {
                print("You cannot place jokers on an empty wagon/locomotive");
            }
            else
            {
                Transform parent = _wagons[index].RedGroup.transform;
                Transform placedDie = parent.GetChild(0);
                float x = placedDie.position.x;
                x += _wagons[index].RedNumber * 1f;
                float y = placedDie.position.y;
                float z = placedDie.position.z;

                var children = PositionJokers("RedPositioned", placedDie.eulerAngles, new Vector3(x, y + 1f, z));
                _wagons[index].RedNumber += children.Count;

                foreach (var child in children)
                {
                    child.SetParent(parent, true);
                    _diceSetupManager.RpcSetParentWithNetId(child.GetComponent<NetworkIdentity>().netId, parent.GetComponent<NetworkIdentity>().netId);
                }

                //_wagons[index].RedNumber += _selectedDiceGroup.transform.childCount;

                _board.SetRedGroup(_selectedDiceGroup.name, true, index);

                _imageAssigner.CheckIfTakesTrunk(client, index);
            }
        }
        else if (waypoint == null || _selectedDiceGroup.name.Contains("JokerGroup"))
        {
            print("Waypoint is null or not available");
        }
        else
        {
            Debug.Log("fourth if");
            MoveSelectedDiceToTile(targetImage.rectTransform.position, -0.5f);
            //Vector3 pos = targetImage.rectTransform.position;
            //pos.x -= 1.6f;
            //pos.y = 1f;
            //pos.z -= 0.15f;
            //_selectedDiceGroup.transform.position = pos;
            ////string tag = _selectedDiceGroup.tag;
            ////_selectedDiceGroup.tag = "Positioned";
            //_turnManager.SetTagPositioned(_selectedDiceGroup);

            //foreach (Transform child in _selectedDiceGroup.transform)
            //{
            //    //string childTag = child.tag;
            //    _turnManager.SetTagPositioned(child.gameObject);
            //    //child.tag = "Positioned";
            //}

            _wagons[index].RedGroup = _selectedDiceGroup;
            _wagons[index].RedNumber += _selectedDiceGroup.transform.childCount;

            _tiles[index].tag = untaggedTile;

            //_imageAssigner.SetImageMaterial(index, MaterialColor.NONE);
            //_imageAssigner.RemoveGreenTile(index);

            _board.Red++;
            _board.SetRedGroup(_selectedDiceGroup.name, true, index);

            waypoint.tag = tileWithDice;
            for (int i = index + 1; i < 16; i++)
            {
                if (!_wagons[i].HasDice())
                {
                    _wagons[i].tag = availableTile;
                    break;
                }
            }

            _imageAssigner.CheckIfTakesTrunk(client, index);
        }

        _imageAssigner.TargetRemoveGreenOutlines(client);

        if (_wagons[index].RedNumber == 7)
        {
            yield return new WaitForSeconds(2f);
            ServerStealWagon(index);
        }
    }

    private void ShiftWagons(int index)
    {
        GameObject currentImageObject = _wagons[index].gameObject;
        Image currentImage = currentImageObject.GetComponent<Image>();
        Image nextImage;
        RectTransform currentImageRect = currentImage.GetComponent<RectTransform>();
        RectTransform nextImageRect;
        Vector3 posCurrent = currentImageRect.position;
        Vector3 posNext;

        GameObject currentTrunk = null;
        Vector3 currentTrunkPos = Vector3.zero;
        GameObject nextTrunk;
        Vector3 nextTrunkPos;
        if (index < 16)
        {
            currentTrunk = _trunks[index];
            currentTrunkPos = currentTrunk.transform.position;
        }

        bool exists;
        for (int i = index - 1; i >= 0; i--)
        {
            StartCoroutine(TimeUtil.Wait());
            do
            {
                nextImage = _imageAssigner.imageObjects[i];
                exists = nextImage != null && nextImage.gameObject.activeInHierarchy;
                if (exists)
                    break;
                i--;
            } while (i >= 0);
            if (exists)
            {
                Debug.Log("exists, i: " + i);
                nextImageRect = nextImage.GetComponent<RectTransform>();
                posNext = nextImageRect.position;
                nextImageRect.position = posCurrent;
                posCurrent = posNext;

                Vector3 pos = nextImage.rectTransform.position;
                MoveDiceToNextTile(pos, i);

                if (_imageAssigner.IsTrunkTaken(i) || index >= 16) continue;

                nextTrunk = _trunks[i];
                if (nextTrunk != null && currentTrunk != null)
                {
                    nextTrunkPos = nextTrunk.transform.position;
                    nextTrunk.transform.position = currentTrunkPos;
                    currentTrunkPos = nextTrunkPos;
                }
            }
        }
    }

    public IEnumerator AttackGreen(NetworkConnectionToClient client, int index, GameObject waypoint, float time = 0.1f)
    {
        yield return new WaitForSeconds(time);

        Image targetImage = waypoint.GetComponent<Image>();
        if (_wagons[index].GreenGroup != null && _wagons[index].GreenGroup.name.Contains(_groupName) && _wagons[index].GreenNumber > 0)
        {
            Transform parent = _wagons[index].GreenGroup.transform;
            StartCoroutine(SetTransformParent(parent));

            //_wagons[index].GreenGroup = parent.gameObject;
            _wagons[index].GreenNumber = parent.childCount;

            _board.SetGreenGroup(_selectedDiceGroup.name, true, index);

            _imageAssigner.CheckIfTakesTrunk(client, index);
        }
        else if (_selectedDiceGroup.name.Contains("JokerGroup") && waypoint.CompareTag(tileWithDice))
        {
            if (_wagons[index].GreenNumber == 0)
            {
                print("You cannot place jokers on an empty wagon/locomotive");
            }
            else
            {
                Transform parent = _wagons[index].GreenGroup.transform;
                Transform placedDie = parent.GetChild(0);
                float x = placedDie.position.x;
                x += _wagons[index].BlueNumber * 1f;
                float y = placedDie.position.y;
                float z = placedDie.position.z;

                var children = PositionJokers("GreenPositioned", placedDie.eulerAngles, new Vector3(x, y + 1f, z));
                _wagons[index].GreenNumber += children.Count;

                foreach (var child in children)
                {
                    child.SetParent(parent, true);
                    _diceSetupManager.RpcSetParentWithNetId(child.GetComponent<NetworkIdentity>().netId, parent.GetComponent<NetworkIdentity>().netId);
                }

                //_wagons[index].GreenNumber += _selectedDiceGroup.transform.childCount;

                _board.SetGreenGroup(_selectedDiceGroup.name, true, index);

                _imageAssigner.CheckIfTakesTrunk(client, index);
            }
        }
        else if (waypoint == null || _selectedDiceGroup.name.Contains("JokerGroup"))
        {
            print("Waypoint is null or not available");
        }
        else
        {
            MoveSelectedDiceToTile(targetImage.rectTransform.position, 0.5f);

            _wagons[index].GreenGroup = _selectedDiceGroup;
            _wagons[index].GreenNumber += _selectedDiceGroup.transform.childCount;

            _tiles[index].tag = untaggedTile;

            //_imageAssigner.SetImageMaterial(index, MaterialColor.NONE);
            //_imageAssigner.RemoveGreenTile(index);

            _board.Green++;
            _board.SetGreenGroup(_selectedDiceGroup.name, true, index);

            waypoint.tag = tileWithDice;
            for (int i = index + 1; i < 16; i++)
            {
                if (!_wagons[i].HasDice())
                {
                    _wagons[i].tag = availableTile;
                    break;
                }
            }

            _imageAssigner.CheckIfTakesTrunk(client, index);
        }

        _imageAssigner.TargetRemoveGreenOutlines(client);

        if (_wagons[index].GreenNumber == 7)
        {
            yield return new WaitForSeconds(2f);
            ServerStealWagon(index);
        }
    }

    public IEnumerator AttackYellow(NetworkConnectionToClient client, int index, GameObject waypoint, float time = 0.1f)
    {
        yield return new WaitForSeconds(time);

        Image targetImage = waypoint.GetComponent<Image>();
        if (_wagons[index].YellowGroup != null && _wagons[index].YellowGroup.name.Contains(_groupName) && _wagons[index].YellowNumber > 0)
        {
            Transform parent = _wagons[index].YellowGroup.transform;
            StartCoroutine(SetTransformParent(parent));

            //_wagons[index].YellowGroup = parent.gameObject;
            _wagons[index].YellowNumber = parent.childCount;

            _board.SetYellowGroup(_selectedDiceGroup.name, true, index);

            _imageAssigner.CheckIfTakesTrunk(client, index);
        }
        else if (_selectedDiceGroup.name.Contains("JokerGroup") && waypoint.CompareTag(tileWithDice))
        {
            if (_wagons[index].YellowNumber == 0)
            {
                print("You cannot place jokers on an empty wagon/locomotive");
            }
            else
            {
                Transform parent = _wagons[index].YellowGroup.transform;
                Transform placedDie = parent.GetChild(0);
                float x = placedDie.position.x;
                x += _wagons[index].BlueNumber * 1f;
                float y = placedDie.position.y;
                float z = placedDie.position.z;

                var children = PositionJokers("YellowPositioned", placedDie.eulerAngles, new Vector3(x, y + 1f, z));
                _wagons[index].YellowNumber += children.Count;

                foreach (var child in children)
                {
                    child.SetParent(parent, true);
                    _diceSetupManager.RpcSetParentWithNetId(child.GetComponent<NetworkIdentity>().netId, parent.GetComponent<NetworkIdentity>().netId);
                }

                //_wagons[index].YellowNumber += _selectedDiceGroup.transform.childCount;

                _board.SetYellowGroup(_selectedDiceGroup.name, true, index);

                _imageAssigner.CheckIfTakesTrunk(client, index);
            }
        }
        else if (waypoint == null || _selectedDiceGroup.name.Contains("JokerGroup"))
        {
            print("Waypoint is null or not available");
        }
        else
        {
            MoveSelectedDiceToTile(targetImage.rectTransform.position, 1.5f);

            _wagons[index].YellowGroup = _selectedDiceGroup;
            _wagons[index].YellowNumber += _selectedDiceGroup.transform.childCount;

            _tiles[index].tag = untaggedTile;

            //_imageAssigner.SetImageMaterial(index, MaterialColor.NONE);
            //_imageAssigner.RemoveGreenTile(index);

            _board.Yellow++;
            _board.SetYellowGroup(_selectedDiceGroup.name, true, index);

            waypoint.tag = tileWithDice;
            for (int i = index + 1; i < 16; i++)
            {
                if (!_wagons[i].HasDice())
                {
                    _wagons[i].tag = availableTile;
                    break;
                }
            }

            _imageAssigner.CheckIfTakesTrunk(client, index);
        }

        _imageAssigner.TargetRemoveGreenOutlines(client);

        if (_wagons[index].YellowNumber == 7)
        {
            yield return new WaitForSeconds(2f);
            ServerStealWagon(index);
        }
    }

    private void MoveDiceToNextTile(Vector3 pos, int i)
    {
        pos.x -= 2.5f;
        pos.y = 1f;
        var startZ = pos.z;
        if (_wagons[i].BlueNumber > 0)
        {
            pos.z = startZ - 1.5f;
            MoveDicePosition(_wagons[i].BlueGroup.transform, pos);
            //_wagons[i].BlueGroup.transform.position = pos;
        }
        if (_wagons[i].RedNumber > 0)
        {
            pos.z = startZ - 0.5f;
            MoveDicePosition(_wagons[i].RedGroup.transform, pos);
            //_wagons[i].RedGroup.transform.position = pos;
        }
        if (_wagons[i].GreenNumber > 0)
        {
            pos.z = startZ + 0.5f;
            MoveDicePosition(_wagons[i].GreenGroup.transform, pos);
            //_wagons[i].GreenGroup.transform.position = pos;
        }
        if (_wagons[i].YellowNumber > 0)
        {
            pos.z = startZ + 1.5f;
            MoveDicePosition(_wagons[i].YellowGroup.transform, pos);
            //_wagons[i].YellowGroup.transform.position = pos;
        }
    }

    private void MoveDicePosition(Transform parent, Vector3 pos)
    {
        foreach (Transform child in parent)
        {
            var childRb = child.GetComponent<Rigidbody>();
            childRb.MovePosition(pos);
            pos.x += 1f;
        }
    }

    private void MoveDiceFromTile(int index)
    {
        _retreatScript.RetreatAllDice(index);

        //GameObject blueDice = _wagons[index].BlueGroup;
        //GameObject redDice = _wagons[index].RedGroup;
        //GameObject greenDice = _wagons[index].GreenGroup;
        //GameObject yellowDice = _wagons[index].YellowGroup;
        //if (blueDice != null)
        //{
        //    Retreat.Instance.RetreatDice(_wagons[index].BlueGroup.GetComponent<NetworkIdentity>().netId);
        //}
        //if (redDice != null)
        //{
        //    Retreat.Instance.RetreatDice(_wagons[index].RedGroup.GetComponent<NetworkIdentity>().netId);
        //}
        //if (greenDice != null)
        //{
        //    Retreat.Instance.RetreatDice(_wagons[index].GreenGroup.GetComponent<NetworkIdentity>().netId);
        //}
        //if (yellowDice != null)
        //{
        //    Retreat.Instance.RetreatDice(_wagons[index].YellowGroup.GetComponent<NetworkIdentity>().netId);
        //}
    }

    private void UseAmbushTrunk(int index)
    {
        Debug.Log("UseAmbushTrunk");
        _imageAssigner.indexes.Add(index);
        if (_imageAssigner.indexes.Count == 2)
        {
            action = PlayerAction.NEUTRAL;

            Debug.Log("Call CmdSwitchTileDice");
            _playerCommandController.CmdSwitchTileDice(_imageAssigner.indexes[0], _imageAssigner.indexes[1]);
        }

    }

    public void AddDefaultEvent(int index)
    {
        if (!enabled)
        {
            Debug.Log("ClickTileEvent script is disabled, skipping event.");
            return;
        }

        if (action == PlayerAction.ATTACK)
            AttackWagon(index);
        else if (action == PlayerAction.STEAL)
            StealWagon(index);
        else if (action == PlayerAction.AMBUSH)
        {
            UseAmbushTrunk(index);
        }
    }

    private void AddEventByScript(GameObject[] gameObjects)
    {
        for (int i = 0; i < gameObjects.Length; i++)
        {
            int index = i;
            if (gameObjects[i].GetComponent<EventTrigger>() == null)
            {
                gameObjects[i].AddComponent<EventTrigger>();
            }
            EventTrigger trigger = gameObjects[i].GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((functionIWant) => { AddDefaultEvent(index); });
            //entry.callback.AddListener((functionIWant) => { ClickEventAndRemove(trigger); });
            //entry.callback.AddListener((functionIWant) => { StealEvent(); });

            trigger.triggers.Add(entry);
        }
    }

    public void SetAction(PlayerAction a)
    {
        action = a;
    }

    public PlayerAction GetAction() { return action; }
}