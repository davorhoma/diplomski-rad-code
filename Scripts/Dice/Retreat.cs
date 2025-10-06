using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Retreat : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField, FormerlySerializedAs("_currentPlayerScript")] private TurnManager _turnManager;
    [SerializeField, FormerlySerializedAs("_imageProperties")] private Wagon[] _wagons;
    [SerializeField] private SelectOutline _selectOutlineScript;
    [SerializeField] private ImageAssigner _imageAssignerScript;
    [SerializeField] private ButtonRoll _buttonRollScript;

    [SerializeField] private ApplyForce _applyForceBlue;
    [SerializeField] private ApplyForce _applyForceRed;
    [SerializeField] private ApplyForce _applyForceGreen;
    [SerializeField] private ApplyForce _applyForceYellow;

    [SerializeField, FormerlySerializedAs("_applyForceMonoBlue")] private DiceActionManager _diceActionManagerBlue;
    [SerializeField, FormerlySerializedAs("_applyForceMonoRed")] private DiceActionManager _diceActionManagerRed;
    [SerializeField, FormerlySerializedAs("_applyForceMonoGreen")] private DiceActionManager _diceActionManagerGreen;
    [SerializeField, FormerlySerializedAs("_applyForceMonoYellow")] private DiceActionManager _diceActionManagerYellow;

    [SerializeField] private Transform _blueDiceStart;
    [SerializeField] private Transform _redDiceStart;
    [SerializeField] private Transform _greenDiceStart;
    [SerializeField] private Transform _yellowDiceStart;

    [SerializeField] private SoundManager _soundManager;

    private RaycastHit _raycastHit;
    private LayerMask _layerMask;

    void Start()
    {
        _layerMask = LayerMask.GetMask("DiceLayer");
    }

    // Update is called once per frame
    //void Update()
    //{
    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    //    if (Physics.Raycast(ray, out _raycastHit, Mathf.Infinity, _layerMask)
    //            && Input.GetMouseButtonDown(0))
    //    {
    //        Rigidbody hitDie = _raycastHit.rigidbody;

    //        if (hitDie != null)
    //        {
    //            GameObject parent = hitDie.transform.parent.gameObject;
    //            RetreatDice(parent);
    //        }
    //    }
    //}

    [Server]
    public void RetreatDice(uint parentNetId, NetworkConnectionToClient conn)
    {
        if (!NetworkClient.spawned.TryGetValue(parentNetId, out var networkIdentity))
        {
            return;
        }

        var parent = networkIdentity.gameObject;
        PlayerColor currentPlayer = _turnManager.Current;

        _selectOutlineScript.TargetDisableOutline(conn, parentNetId);

        switch (currentPlayer)
        {
            case PlayerColor.BLUE:
                if (parent.tag != "BluePositioned")
                {
                    return;
                }

                Debug.Log("Blue dice to corner");
                StartCoroutine(_diceActionManagerBlue.StealDiceToCorner(parent, _blueDiceStart.position));
                foreach (var property in _wagons)
                {
                    if (property.BlueGroup == parent)
                    {
                        property.BlueNumber = 0;
                        break;
                    }
                }
                _imageAssignerScript.ClearBlueGroup(parent.name);
                for (int i = 0; i < _wagons.Length; i++)
                {
                    if (_wagons[i].HasDice())
                    {
                        _buttonRollScript.TargetRetreatDice(conn, _wagons[i].BlueGroup.GetComponent<NetworkIdentity>().netId, i);
                        break;
                    }
                }
                break;
            case PlayerColor.RED:
                if (parent.tag != "RedPositioned")
                {
                    return;
                }
                StartCoroutine(_diceActionManagerRed.StealDiceToCorner(parent, _redDiceStart.position));
                foreach (var property in _wagons)
                {
                    if (property.RedGroup == parent)
                    {
                        property.RedNumber = 0;
                        break;
                    }
                }
                _imageAssignerScript.ClearRedGroup(parent.name);
                for (int i = 0; i < _wagons.Length; i++)
                {
                    if (_wagons[i].HasDice())
                    {
                        _buttonRollScript.TargetRetreatDice(conn, _wagons[i].RedGroup.GetComponent<NetworkIdentity>().netId, i);
                        break;
                    }
                }
                break;
            case PlayerColor.GREEN:
                if (parent.tag != "GreenPositioned")
                {
                    return;
                }
                StartCoroutine(_diceActionManagerGreen.StealDiceToCorner(parent, _greenDiceStart.position));
                foreach (var property in _wagons)
                {
                    if (property.GreenGroup == parent)
                    {
                        property.GreenNumber = 0;
                        break;
                    }
                }
                _imageAssignerScript.ClearGreenGroup(parent.name);
                for (int i = 0; i < _wagons.Length; i++)
                {
                    if (_wagons[i].HasDice())
                    {
                        _buttonRollScript.TargetRetreatDice(conn, _wagons[i].GreenGroup.GetComponent<NetworkIdentity>().netId, i);
                        break;
                    }
                }
                break;
            case PlayerColor.YELLOW:
                if (parent.tag != "YellowPositioned")
                {
                    return;
                }
                StartCoroutine(_diceActionManagerYellow.StealDiceToCorner(parent, _yellowDiceStart.position));
                foreach (var property in _wagons)
                {
                    if (property.YellowGroup == parent)
                    {
                        property.YellowNumber = 0;
                        break;
                    }
                }
                _imageAssignerScript.ClearYellowGroup(parent.name);
                for (int i = 0; i < _wagons.Length; i++)
                {
                    if (_wagons[i].HasDice())
                    {
                        _buttonRollScript.TargetRetreatDice(conn, _wagons[i].YellowGroup.GetComponent<NetworkIdentity>().netId, i);
                        break;
                    }
                }
                break;
        }

        _soundManager.PlaySoundGrabDice();
        //_buttonRollScript.TargetRetreatDice(conn);
    }

    public void RetreatAllDice(int index)
    {
        var property = _wagons[index];
        if (property.BlueNumber > 0 && _wagons[index].BlueGroup != null)
        {
            StartCoroutine(_diceActionManagerBlue.StealDiceToCorner(property.BlueGroup, _blueDiceStart.position));
            _imageAssignerScript.ClearBlueGroup(property.BlueGroup.name);
            _imageAssignerScript.RpcClearBlueGroup(property.BlueGroup.name);
        }
        if (property.RedNumber > 0)
        {
            Debug.Log("Retreat red");
            StartCoroutine(_diceActionManagerRed.StealDiceToCorner(property.RedGroup, _redDiceStart.position));
            _imageAssignerScript.ClearRedGroup(property.RedGroup.name);
            _imageAssignerScript.RpcClearRedGroup(property.RedGroup.name);
        }
        if (property.GreenNumber > 0)
        {
            Debug.Log("Retreat green");
            StartCoroutine(_diceActionManagerGreen.StealDiceToCorner(property.GreenGroup, _greenDiceStart.position));
            _imageAssignerScript.ClearGreenGroup(property.GreenGroup.name);
            _imageAssignerScript.RpcClearGreenGroup(property.GreenGroup.name);
        }
        if (property.YellowNumber > 0)
        {
            Debug.Log("Retreat yellow");
            StartCoroutine(_diceActionManagerYellow.StealDiceToCorner(property.YellowGroup, _yellowDiceStart.position));
            _imageAssignerScript.ClearYellowGroup(property.YellowGroup.name);
            _imageAssignerScript.RpcClearYellowGroup(property.YellowGroup.name);
        }

        _soundManager.PlaySoundGrabDice();
        property.ResetAfterSteal();
        property.RpcResetAfterSteal();
    }

    public void RetreatDiceWithBomb(int index)
    {
        var property = _wagons[index];
        if (property.BlueNumber > 0 && _wagons[index].BlueGroup != null)
        {
            StartCoroutine(_diceActionManagerBlue.RetreatDiceWithBombToCorner(property.BlueGroup, _blueDiceStart.position));
            property.BlueNumber = 1;
        }
        if (property.RedNumber > 0)
        {
            Debug.Log("Retreat red");
            StartCoroutine(_diceActionManagerRed.RetreatDiceWithBombToCorner(property.RedGroup, _redDiceStart.position));
            property.RedNumber = 1;
        }
        if (property.GreenNumber > 0)
        {
            Debug.Log("Retreat green");
            StartCoroutine(_diceActionManagerGreen.RetreatDiceWithBombToCorner(property.GreenGroup, _greenDiceStart.position));
            property.GreenNumber = 1;
        }
        if (property.YellowNumber > 0)
        {
            Debug.Log("Retreat yellow");
            StartCoroutine(_diceActionManagerYellow.RetreatDiceWithBombToCorner(property.YellowGroup, _yellowDiceStart.position));
            property.YellowNumber = 1;
        }

        _soundManager.PlaySoundGrabDice();
    }
}
