using Mirror;
using Mirror.BouncyCastle.Asn1.Ocsp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DiceActionManager : NetworkBehaviour
{
    private float _rollForce = 15f;
    private float _torqueAmount = 100f;
    [SerializeField] LayerMask _layerMask;

    private Vector3 _blueCornerPosition = new Vector3(2, 2, -5);
    private Vector3 _redCornerPosition = new Vector3(-11.8f, 0.8f, -5.4f);
    private Vector3 _greenCornerPosition = new Vector3(-11.8f, 0.8f, -5.9f);
    private Vector3 _yellowCornerPosition = new Vector3(-11.8f, 0.8f, -6.4f);

    [SerializeField] float x = -4.401576f;

    [SyncVar] private GameObject lastHatGroup = null;
    [SyncVar] private GameObject lastMaskGroup = null;
    [SyncVar] private GameObject lastHeelGroup = null;
    [SyncVar] private GameObject lastHorseshoeGroup = null;
    [SyncVar] private GameObject lastGunsGroup = null;
    [SyncVar] private GameObject lastJokerGroup = null;
    private List<Transform> stolenDice = new List<Transform>();

    private int groupIndex;

    public bool _rolling;
    public Transform _transform;
    public List<Rigidbody> allRigidbodies;

    [SerializeField] private SelectOutline _selectOutlineScript;
    [SerializeField] private ClickTileEvent _clickTileScript;
    [SerializeField] private Players _playersScript;
    [SerializeField, FormerlySerializedAs("_currentPlayerScript")] private TurnManager _turnManager;
    private PlayerColor _currentPlayer;

    private string _selectableTag = "Selectable";
    private string _blueSelectable = "BlueSelectable";
    private string _redSelectable = "RedSelectable";
    private string _greenSelectable = "GreenSelectable";
    private string _yellowSelectable = "YellowSelectable";

    [SerializeField] Button _buttonEndTurn;

    [SerializeField] private List<GameDie> gameDice = new List<GameDie>();

    [SerializeField] private DicePositioner _dicePositioner;
    [SerializeField] private GameObject[] _parentGroupPrefabs;
    [SerializeField, FormerlySerializedAs("_diceManager")] private DiceSetupManager _diceSetupManager;

    [SerializeField] private SoundManager _soundManager;

    private IEnumerator Wait(float time = 0.5f)
    {
        yield return new WaitForSeconds(time);
    }

    public IEnumerator RollDice(float force, float torque)
    {
        yield return StartCoroutine(Wait());
        _rollForce = force;
        _torqueAmount = torque;
        Transform parent = gameObject.transform;
        _currentPlayer = _turnManager.Current;

        var found = GameObject.FindGameObjectsWithTag(_currentPlayer.ToString());

        if (found.Length > 0)
        {
            allRigidbodies = _playersScript.GetRigidBodies(_currentPlayer);

            foreach (var child in found)
            {
                gameDice.Add(child.GetComponent<GameDie>());
            }
        }
        else if (parent.childCount > 0)
        {
            allRigidbodies.Clear();
            Debug.Log("parent.childCount > 0");
            foreach (Transform child in parent.transform)
            {
                allRigidbodies.Add(child.GetComponent<Rigidbody>());
                gameDice.Add(child.GetComponent<GameDie>());
            }
        }
        else
        {
            List<Transform> childrenToSetParent = new List<Transform>();
            if (stolenDice.Count > 0)
            {
                foreach (Transform child in stolenDice)
                {
                    AddRigidbody(childrenToSetParent, child);
                }

                stolenDice.Clear();
            }
            if (lastHatGroup != null && lastHatGroup.tag.Contains(_selectableTag))
            {
                foreach (Transform child in lastHatGroup.transform)
                {
                    AddRigidbody(childrenToSetParent, child);
                }
            }
            if (lastMaskGroup != null && lastMaskGroup.tag.Contains(_selectableTag))
            {
                foreach (Transform child in lastMaskGroup.transform)
                {
                    AddRigidbody(childrenToSetParent, child);
                }
            }
            if (lastHeelGroup != null && lastHeelGroup.tag.Contains(_selectableTag))
            {
                foreach (Transform child in lastHeelGroup.transform)
                {
                    AddRigidbody(childrenToSetParent, child);
                }
            }
            if (lastHorseshoeGroup != null && lastHorseshoeGroup.tag.Contains(_selectableTag))
            {
                foreach (Transform child in lastHorseshoeGroup.transform)
                {
                    AddRigidbody(childrenToSetParent, child);
                }
            }
            if (lastGunsGroup != null && lastGunsGroup.tag.Contains(_selectableTag))
            {
                foreach (Transform child in lastGunsGroup.transform)
                {
                    AddRigidbody(childrenToSetParent, child);
                }
            }
            if (lastJokerGroup != null && lastJokerGroup.tag.Contains(_selectableTag))
            {
                foreach (Transform child in lastJokerGroup.transform)
                {
                    AddRigidbody(childrenToSetParent, child);
                }
            }

            //float a = 0;
            foreach (Transform child in childrenToSetParent)
            {
                _turnManager.SetTagSelectable(child.gameObject);
                child.SetParent(parent, true);
                var childRb = child.GetComponent<Rigidbody>();
                //childRb.MovePosition(new Vector3(a, 8.224f, 0));
                //a += 1f;

                gameDice.Add(child.GetComponent<GameDie>());
            }

            // Maybe create a new variable instead of assigning it to allRigidbodies
            allRigidbodies = GetRigidbodiesFromTransforms(childrenToSetParent);

            if (parent.childCount == 0)
            {
                Debug.Log("There are no dice to roll");
                yield break;
            }
        }

        if (allRigidbodies.Count == 0)
        {
            Debug.Log("There are no dice to roll");
            yield break;
        }

        _soundManager.PlaySoundGrabDice();
        _dicePositioner.PositionDiceForRoll(allRigidbodies);

        yield return StartCoroutine(AddForceAndTorque());
    }

    private List<Rigidbody> GetRigidbodiesFromTransforms(List<Transform> list)
    {
        var rigidbodies = new List<Rigidbody>();
        foreach (var child in list)
        {
            var childRb = child.GetComponent<Rigidbody>();
            rigidbodies.Add(childRb);
        }

        return rigidbodies;
    }

    IEnumerator AddForceAndTorque()
    {
        //Debug.Log("AddForceAndTorque");
        var timeOut = Time.time + 10f;

        // Wait for all dice to stop rolling
        while (Time.time < timeOut && !AreAllDiceSleeping())
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        Debug.Log("Passed");

        foreach (GameDie gameDie in gameDice)
        {
            gameDie.enabled = true;
            gameDie.Roll();
        }

        // Wait for dice to stop
        while (gameDice.Any(r => r.Value == 0))
        {
            yield return null;
        }

        Debug.Log("Rolled dice");

        StartCoroutine(ReadAndProcessDiceValues());
    }

    IEnumerator ReadAndProcessDiceValues()
    {
        Debug.Log("all dice have value");
        // Ensure a slight delay after all dice have stopped to stabilize their positions
        yield return new WaitForSeconds(0.1f);

        List<int> dieValues = new List<int>();
        List<GameDie> gameDiceToRemove = new List<GameDie>();
        foreach (GameDie gameDie in gameDice)
        {
            if (gameDie.enabled == false)
            {
                gameDiceToRemove.Add(gameDie);
            }
        }

        foreach (GameDie r in gameDiceToRemove)
        {
            gameDice.Remove(r);
        }

        foreach (GameDie gameDie in gameDice)
        {
            if (gameDie.Value > 0)
            {
                Debug.Log("Dice value:" + gameDie.Value);
                dieValues.Add((int)gameDie.Value);
                gameDie.enabled = false;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        gameDice.Clear();

        Debug.Log("dieValues.Count: " + dieValues.Count);
        Debug.Log("allRigidbodies.Count: " + allRigidbodies.Count);
        if (dieValues.Count == allRigidbodies.Count)
        {
            // Process the die values as needed
            ProcessDieValues(dieValues);
            allRigidbodies.Clear();
            _turnManager.SetSelectionTag();
            //_selectOutlineScript.enabled = true;
            _clickTileScript.enabled = true;
            _clickTileScript.SetAction(PlayerAction.ATTACK);
            _turnManager.RpcUpdateScriptsActivity();
        }
        else
        {
            Debug.Log("dieValues.Count == allRigidbodies.Count");
        }
    }

    IEnumerator WaitForDiceToStop()
    {
        //Debug.Log("WAITING");
        //var timeOut = Time.time + 10f;

        // Wait for all dice to stop rolling
        //while (!AreAllDiceSleepingAndHaveValue())
        //{
        //    //Debug.Log("Not all dice are sleeping");
        //    yield return null;
        //}

        Debug.Log("all dice have value");
        // Ensure a slight delay after all dice have stopped to stabilize their positions
        yield return new WaitForSeconds(0.1f);

        List<int> dieValues = new List<int>();
        List<GameDie> gameDiceToRemove = new List<GameDie>();
        foreach (GameDie gameDie in gameDice)
        {
            if (gameDie.enabled == false)
            {
                gameDiceToRemove.Add(gameDie);
            }
        }

        foreach (GameDie r in gameDiceToRemove)
        {
            gameDice.Remove(r);
        }

        foreach (GameDie gameDie in gameDice)
        {
            if (gameDie.Value > 0)
            {
                Debug.Log("Dice value:" + gameDie.Value);
                dieValues.Add((int)gameDie.Value);
                gameDie.enabled = false;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        gameDice.Clear();

        Debug.Log("dieValues.Count: " + dieValues.Count);
        Debug.Log("allRigidbodies.Count: " + allRigidbodies.Count);
        if (dieValues.Count == allRigidbodies.Count)
        {
            // Process the die values as needed
            ProcessDieValues(dieValues);
            allRigidbodies.Clear();
            _turnManager.SetSelectionTag();
            //_selectOutlineScript.enabled = true;
            _clickTileScript.enabled = true;
            _clickTileScript.SetAction(PlayerAction.ATTACK);
            _turnManager.RpcUpdateScriptsActivity();
        }
        else
        {
            Debug.Log("dieValues.Count == allRigidbodies.Count");
        }
    }

    private bool AreAllDiceSleeping()
    {
        foreach (GameDie gameDie in gameDice)
        {
            if (!gameDie.IsDieSleeping())
            {
                //Debug.Log("false");
                return false;
            }
        }

        //Debug.Log("true");
        return true;
    }

    public bool AreAllDiceSleepingAndHaveValue()
    {
        foreach (GameDie gameDie in gameDice)
        {
            if (!gameDie.IsDieSleeping() && !gameDie.HasDieValue())
            {
                return false;
            }
        }
        for (int i = 0; i < gameDice.Count; i++)
        {
            Debug.Log(gameDice[i].Value);
        }
        return true;
    }

    void ProcessDieValues(List<int> dieValues)
    {
        int jokers = 0, hats = 0, masks = 0, heels = 0, guns = 0, horseshoes = 0;

        List<Rigidbody> maskRigidbodies = new List<Rigidbody>();
        List<Rigidbody> hatRigidbodies = new List<Rigidbody>();
        List<Rigidbody> heelRigidbodies = new List<Rigidbody>();
        List<Rigidbody> gunRigidbodies = new List<Rigidbody>();
        List<Rigidbody> horseshoeRigidbodies = new List<Rigidbody>();
        List<Rigidbody> jokerRigidbodies = new List<Rigidbody>();
        for (int i = 0; i < dieValues.Count; i++)
        {
            switch (dieValues[i])
            {
                case 1:
                    jokerRigidbodies.Add(allRigidbodies[i]);
                    jokers++;
                    break;
                case 2:
                    hatRigidbodies.Add(allRigidbodies[i]);
                    hats++;
                    break;
                case 3:
                    maskRigidbodies.Add(allRigidbodies[i]);
                    masks++;
                    break;
                case 4:
                    heelRigidbodies.Add(allRigidbodies[i]);
                    heels++;
                    break;
                case 5:
                    gunRigidbodies.Add(allRigidbodies[i]);
                    guns++;
                    break;
                case 6:
                    horseshoeRigidbodies.Add(allRigidbodies[i]);
                    horseshoes++;
                    break;
            }

            var tag = SetChildTag(allRigidbodies[i].transform);
            _playersScript.SetTag(allRigidbodies[i].transform, tag);
        }

        List<int> values = new List<int>();
        _clickTileScript.AppendDice = false;

        string color = GetColor();
        ProcessDiceGroup($"{color}HatGroup{groupIndex}", ref lastHatGroup, hatRigidbodies, hats, 0);
        ProcessDiceGroup($"{color}MaskGroup{groupIndex}", ref lastMaskGroup, maskRigidbodies, masks, 1);
        ProcessDiceGroup($"{color}HeelGroup{groupIndex}", ref lastHeelGroup, heelRigidbodies, heels, 2);
        ProcessDiceGroup($"{color}HorseshoeGroup{groupIndex}", ref lastHorseshoeGroup, horseshoeRigidbodies, horseshoes, 3);
        ProcessDiceGroup($"{color}GunsGroup{groupIndex}", ref lastGunsGroup, gunRigidbodies, guns, 4);
        ProcessDiceGroup($"{color}JokerGroup{groupIndex}", ref lastJokerGroup, jokerRigidbodies, jokers, 5);
        Debug.Log("Processed all dice groups");

        groupIndex++;
        x = -4.401576f;

        //print("Setting button end turn");
        _buttonEndTurn.interactable = true;

        if (values.Count == 0) return;
        values.Sort();
    }

    private string GetColor()
    {
        if (_currentPlayer == PlayerColor.BLUE)
        {
            return "Blue";
        }
        else if (_currentPlayer == PlayerColor.RED)
        {
            return "Red";
        }
        else if ( _currentPlayer == PlayerColor.GREEN)
        {
            return "Green";
        }
        else
        {
            return "Yellow";
        }
    }

    private void ProcessDiceGroup(string groupName, ref GameObject lastGroup, List<Rigidbody> rigidbodies, int count, int groupIndex)
    {
        //Destroy the last created group
        if (lastGroup != null)
        {
            _clickTileScript.AppendDice = true;
            string pattern = @"^[a-zA-Z]+";
            Match match = Regex.Match(groupName, pattern);

            // Check if the match is successful
            if (match.Success)
            {
                Debug.Log("MATCH SUCCESS");

                string result = match.Value;
                _clickTileScript.GroupName = result;
            }

            if (lastGroup.transform.childCount == 0) Destroy(lastGroup);
        }

        if (count > 0)
        {
            GameObject groupObject = CreateGroup(groupName, rigidbodies.Count, groupIndex);

            GroupObjects(groupObject, rigidbodies);

            //Debug.Log($"You rolled {count} {groupName.ToLower()}", gameObject);

            lastGroup = groupObject;

            ResetDice(rigidbodies, x, groupObject);
            x += count + 1f;
        }
    }

    private GameObject CreateGroup(string groupName, int count, int groupIndex)
    {
        GameObject groupObject = Instantiate(_parentGroupPrefabs[groupIndex]);
        groupObject.name = groupName;
        BoxCollider boxCollider = groupObject.AddComponent<BoxCollider>();
        Rigidbody rb = groupObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.mass = count * 1f;

        var tag = _turnManager.SetTagSelectable(groupObject);

        int layerIndex = LayerMask.NameToLayer("DiceGroupLayer");
        groupObject.layer = layerIndex;
        boxCollider.isTrigger = true;

        NetworkServer.Spawn(groupObject);
        //Debug.Log("Created group net id: " + groupObject.GetComponent<NetworkIdentity>().netId);
        _playersScript.SetTag(groupObject.transform, tag);

        return groupObject;
    }

    void GroupObjects(GameObject groupObject, List<Rigidbody> rigidbodies)
    {
        foreach (Rigidbody rb in rigidbodies)
        {
            // Set the groupObject as the parent of each Rigidbody
            //rb.transform.parent = groupObject.transform;
            rb.transform.SetParent(groupObject.transform, true);
        }

        // Rpc set child parent
        foreach (Rigidbody rb in rigidbodies)
        {
            _diceSetupManager.RpcSetParentWithNetId(rb.GetComponent<NetworkIdentity>().netId, groupObject.GetComponent<NetworkIdentity>().netId);
        }
    }

    void ResetDice(List<Rigidbody> rigidbodies, float x, GameObject groupObject, float z = -10f)
    {
        float a = 0f;
        foreach (Rigidbody rb in rigidbodies)
        {
            Vector3 existingRotation = rb.transform.rotation.eulerAngles;
            existingRotation.y = 0f;
            rb.transform.rotation = Quaternion.Euler(existingRotation);
            rb.useGravity = true;

            //rb.MovePosition(new Vector3(a, 1.5f, z));
            rb.transform.position = new Vector3(x + a, 1.5f, z);
            a += 1f;
        }
        //groupObject.transform.position = new Vector3(x, 1.5f, z);
    }

    [Server]
    public IEnumerator SetDiceInCorner(Vector3 startPosition)
    {
        yield return new WaitForSeconds(0.1f);

        SetAllRigidbody(parent => 
        {
            if (parent.childCount > 0)
            {
                //Debug.Log("Setting parent");
                //SetParentPosition(parent);
            }
        }, startPosition);
    }

    public IEnumerator StealDiceToCorner(GameObject stealDice, Vector3 startPosition)
    {
        // Add steal dice to list
        foreach (Transform child in stealDice.transform)
        {
            stolenDice.Add(child);
        }

        Debug.Log("stolenDice.Count" + stolenDice.Count);

        yield return StartCoroutine(MoveDiceToCorner(startPosition));
    }

    public IEnumerator RetreatDiceWithBombToCorner(GameObject retreatDice, Vector3 startPosition)
    {
        int i = 0;
        foreach (Transform child in retreatDice.transform)
        {
            if (i++ == 0) continue;
            stolenDice.Add(child);
        }

        Debug.Log("stolenDice.Count" + stolenDice.Count);

        yield return StartCoroutine(MoveDiceToCorner(startPosition));
    }

    private IEnumerator MoveDiceToCorner(Vector3 startPosition)
    {
        Transform parent = gameObject.transform;

        if (parent.childCount > 0)
        {
            Debug.Log("If");

            //Set parent and position each steal die
            float a = parent.childCount;
            Transform alreadyChild = parent.GetChild(0);

            foreach (Transform child in stolenDice)
            {
                var childRb = child.GetComponent<Rigidbody>();
                childRb.MovePosition(new Vector3(alreadyChild.position.x + a, alreadyChild.position.y, alreadyChild.position.z));

                SetChildTag(child);
                allRigidbodies.Add(child.GetComponent<Rigidbody>());
                a += 1f;
            }

            yield return new WaitForSeconds(0.1f);
            foreach (Transform child in stolenDice)
            {
                child.SetParent(parent, true);
                _diceSetupManager.RpcSetParent(child.GetComponent<NetworkIdentity>().netId, parent.name);
            }
        }
        else
        {
            Debug.Log("Else");
            float a = 0;
            foreach (Transform child in stolenDice)
            {
                var childRb = child.GetComponent<Rigidbody>();
                childRb.MovePosition(new Vector3(startPosition.x + a, startPosition.y, startPosition.z));
                //childRb.position = new Vector3(startPosition.x + a, startPosition.y, startPosition.z);

                SetChildTag(child);
                allRigidbodies.Add(childRb);
                a += 1f;
            }

            yield return new WaitForSeconds(0.1f);
            foreach (Transform child in stolenDice)
            {
                child.SetParent(parent, true);
                _diceSetupManager.RpcSetParent(child.GetComponent<NetworkIdentity>().netId, parent.name);
            }

            //SetParentPosition(parent);
        }

        stolenDice.Clear();
    }

    private string SetChildTag(Transform child)
    {
        if (_currentPlayer == PlayerColor.BLUE)
        {
            child.tag = _blueSelectable;
        }
        else if (_currentPlayer == PlayerColor.RED)
        {
            child.tag = _redSelectable;
        }
        else if (_currentPlayer == PlayerColor.GREEN)
        {
            child.tag = _greenSelectable;
        }
        else
        {
            child.tag = _yellowSelectable;
        }

        return child.tag;
    }

    private void SetParentPosition(Transform parent)
    {
        if (_currentPlayer == PlayerColor.BLUE)
        {
            Debug.Log("Setting parent position for blue player");
            Debug.Log("Parent current pos: " + parent.position.x + ", " + parent.position.y + ", " + parent.position.z);
            parent.position = _blueCornerPosition;
            Debug.Log("Parent pos after: " + parent.position.x + ", " + parent.position.y + ", " + parent.position.z);
        }
        else if (_currentPlayer == PlayerColor.RED)
        {
            parent.position = _redCornerPosition;
        }
        else if (_currentPlayer == PlayerColor.GREEN)
        {
            parent.position = _greenCornerPosition;
        }
        else
        {
            parent.position = _yellowCornerPosition;
        }

        float a = 0f;
        foreach (Transform child in parent.transform)
        {
            child.position = new Vector3(-50f + a, 1f, -7.5f);
            a += 1f;
        }
    }

    [Server]
    private void SetAllRigidbody(System.Action<Transform> onComplete, Vector3 startPosition)
    {
        Transform parent = gameObject.transform;

        allRigidbodies.Clear();
        List<Transform> childrenToSetParent = new List<Transform>();
        //if (lastHatGroup != null && lastHatGroup.tag.Contains("Selectable"))
        //    foreach (Transform child in lastHatGroup.transform)
        //    {
        //        childrenToSetParent.Add(child);
        //        //child.SetParent(parent, false);
        //        var childRb = child.GetComponent<Rigidbody>();
        //        childRb.useGravity = false;
        //        allRigidbodies.Add(childRb);
        //        print("hats set");
        //    }
        //if (lastMaskGroup != null && lastMaskGroup.tag.Contains("Selectable"))
        //    foreach (Transform child in lastMaskGroup.transform)
        //    {
        //        //child.SetParent(parent, false);
        //        AddRigidbody(childrenToSetParent, child);
        //    }
        //if (lastHeelGroup != null && lastHeelGroup.tag.Contains("Selectable"))
        //    foreach (Transform child in lastHeelGroup.transform)
        //    {
        //        AddRigidbody(childrenToSetParent, child);
        //        print("heels set");
        //    }
        //if (lastHorseshoeGroup != null && lastHorseshoeGroup.tag.Contains("Selectable"))
        //    foreach (Transform child in lastHorseshoeGroup.transform)
        //    {
        //        AddRigidbody(childrenToSetParent, child);
        //        print("horseshoes set");
        //    }
        //if (lastGunsGroup != null && lastGunsGroup.tag.Contains("Selectable"))
        //    foreach (Transform child in lastGunsGroup.transform)
        //    {
        //        AddRigidbody(childrenToSetParent, child);
        //        print("guns set");
        //    }
        //if (lastJokerGroup != null && lastJokerGroup.tag.Contains("Selectable"))
        //    foreach (Transform child in lastJokerGroup.transform)
        //    {
        //        AddRigidbody(childrenToSetParent, child);
        //        print("joker set");
        //    }

        var tag = _turnManager.GetCurrentSelectableTag();
        var found = GameObject.FindGameObjectsWithTag(tag);
        //Debug.Log("found.Length = " + found.Length);
        foreach (var obj in found)
        {
            if (obj.transform.parent == null)
            {
                Debug.Log(obj.name);
                continue;
            }
            AddRigidbody(childrenToSetParent, obj.transform);
        }
        
        StartCoroutine(TransformPositions(childrenToSetParent, parent, onComplete, startPosition));
    }

    [Server]
    private IEnumerator TransformPositions(List<Transform> childrenToSetParent, Transform parent, System.Action<Transform> onComplete, Vector3 startPosition)
    {
        yield return StartCoroutine(Wait(0.1f));

        //float a = parent.childCount * 0.45f;
        float a = 0;
        foreach (Transform child in childrenToSetParent)
        {
            //child.parent = parent;
            //Debug.Log("Child current pos: " + child.position.x + ", " + child.position.y + ", " + child.position.z);
            //child.position = new Vector3(startPosition.x + a, startPosition.y, startPosition.z);
            child.GetComponent<Rigidbody>().MovePosition(new Vector3(startPosition.x + a, startPosition.y, startPosition.z));
            //child.SetParent(parent, true);
            //child.transform.localScale = new Vector3(25, 25, 25);
            //child.transform.localPosition = new Vector3(a, 0, 0);
            a += 1f;
            //Debug.Log("Child after pos: " + child.position.x + ", " + child.position.y + ", " + child.position.z);
        }

        yield return StartCoroutine(Wait(0.1f));
        foreach (Transform child in childrenToSetParent)
        {
            child.SetParent(parent, true);
            _diceSetupManager.RpcSetParent(child.GetComponent<NetworkIdentity>().netId, parent.name);
        }

        ActivateGravities(allRigidbodies);

        onComplete?.Invoke(parent);
    }

    private void AddRigidbody(List<Transform> childrenToSetParent, Transform child)
    {
        childrenToSetParent.Add(child);
        var childRb = child.GetComponent<Rigidbody>();
        childRb.useGravity = false;
        allRigidbodies.Add(childRb);
        //print("masks set");
    }

    private void ActivateGravities(List<Rigidbody> rigidbodies)
    {
        foreach (var rb in rigidbodies)
        {
            rb.useGravity = true;
        }
    }
}
