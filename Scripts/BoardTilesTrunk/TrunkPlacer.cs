using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TrunkPlacer : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Transform[] _trunkTransforms;
    private List<Transform> _blueTrunks = new List<Transform>();
    private List<Transform> _redTrunks = new List<Transform>();
    private List<Transform> _greenTrunks = new List<Transform>();
    private List<Transform> _yellowTrunks = new List<Transform>();

    [SerializeField] private GameObject _playerTrunks;
    private Vector3 _playerTrunksPosition;
    private Vector3 _moveToSide = new Vector3(0f, 0f, 1.15f);

    private List<int> _blueTrunksTaken = new List<int>();
    private List<int> _redTrunksTaken = new List<int>();
    private List<int> _greenTrunksTaken = new List<int>();
    private List<int> _yellowTrunksTaken = new List<int>();
    // Ljivo +0.026f, pravo -0.026f

    [SerializeField, FormerlySerializedAs("_currentPlayerScript")] private TurnManager _turnManager;
    [SerializeField] private Retreat _retreatScript;
    [SerializeField] private ImageAssigner _imageAssigner;
    [SerializeField] private ClickTileEvent _clickTileEventScript;
    [SerializeField] private ButtonRoll _buttonRollScript;
    private PlayerCommandController _playerCommandController;

    private RaycastHit _raycastHit;
    private LayerMask _layerMask;
    private GameObject[] _imagesForSwitch = new GameObject[2];
    private int chosenTiles = 0;

    private GameObject _trunkToDisable;
    public GameObject TrunkToDisable {  get { return _trunkToDisable; } set {  _trunkToDisable = value; } }

    private void Awake()
    {
        _layerMask = LayerMask.GetMask("TilesLayer");
    }

    private void Start()
    {
        _playerTrunksPosition = _playerTrunks.transform.position;
        _playerCommandController = NetworkClient.localPlayer.GetComponent<PlayerCommandController>();
    }

    public void PositionTrunkDown(int i)
    {
        if (_trunkTransforms[i] == null)
        {
            var trunk = GameObject.Find("Trunk" + i);
            _trunkTransforms[i] = trunk.transform;
        }

        switch (_turnManager.Current)
        {
            case PlayerColor.BLUE:
                _trunkTransforms[i].position = _playerTrunksPosition - _blueTrunksTaken.Count * _moveToSide;

                foreach (Transform t in _blueTrunks)
                {
                    t.position += _moveToSide;
                }
                //_blueTrunks.Add(_trunkTransforms[i]);
                //_blueTrunksTaken.Add(i);
                break;
            case PlayerColor.RED:
                _trunkTransforms[i].position = _playerTrunksPosition - _redTrunksTaken.Count * _moveToSide;
                foreach (Transform t in _redTrunks)
                {
                    t.position += _moveToSide;
                }
                //_redTrunks.Add(_trunkTransforms[i]);
                //_redTrunksTaken.Add(i);
                break;
            case PlayerColor.GREEN:
                _trunkTransforms[i].position = _playerTrunksPosition - _greenTrunksTaken.Count * _moveToSide;
                foreach (Transform t in _greenTrunks)
                {
                    t.position += _moveToSide;
                }
                //_greenTrunks.Add(_trunkTransforms[i]);
                //_greenTrunksTaken.Add(i);
                break;
            case PlayerColor.YELLOW:
                _trunkTransforms[i].position = _playerTrunksPosition - _yellowTrunksTaken.Count * _moveToSide;
                foreach (Transform t in _yellowTrunks)
                {
                    t.position += _moveToSide;
                }
                //_yellowTrunks.Add(_trunkTransforms[i]);
                //_yellowTrunksTaken.Add(i);
                break;
        }

        _playerCommandController.CmdUpdateTakenTrunks(_trunkTransforms[i], i);
        //for (int j = 0; j < _trunksTaken.Count; j++)
        //{
        //    _trunkTransforms[j].localPosition += _moveToSide;
        //}


        //_trunksTaken.Add(i);
    }

    [Server]
    public void UpdateTakenTrunks(Transform transform, int i)
    {
        switch (_turnManager.Current)
        {
            case PlayerColor.BLUE:
                _blueTrunks.Add(transform);
                _blueTrunksTaken.Add(i);
                break;
            case PlayerColor.RED:
                _redTrunks.Add(transform);
                _redTrunksTaken.Add(i);
                break;
            case PlayerColor.GREEN:
                _greenTrunks.Add(transform);
                _greenTrunksTaken.Add(i);
                break;
            case PlayerColor.YELLOW:
                _yellowTrunks.Add(transform);
                _yellowTrunksTaken.Add(i);
                break;
        }
    }

    public void AdjustTrunkPositions(string name)
    {
        int usedTrunk = 0;
        int trunkIndexToDestroy = -1;

        switch (_turnManager.Current)
        {
            case PlayerColor.BLUE:
                {
                    for (int i = 0; i < _blueTrunksTaken.Count; i++)
                    {
                        if (name == _trunkTransforms[_blueTrunksTaken[i]].name)
                        {
                            usedTrunk = i;
                            break;
                        }
                    }

                    for (int i = usedTrunk + 1; i < _blueTrunksTaken.Count; i++)
                    {
                        _trunkTransforms[_blueTrunksTaken[i]].localPosition += 2 * _moveToSide;
                    }

                    trunkIndexToDestroy = _blueTrunksTaken[usedTrunk];
                    _blueTrunksTaken.Remove(usedTrunk);
                    _blueTrunks.Remove(_trunkTransforms[trunkIndexToDestroy]);

                    //Destroy(_trunkTransforms[trunkToDestroy].gameObject, 1.5f);
                    break;
                }
            case PlayerColor.RED:
                {
                    for (int i = 0; i < _redTrunksTaken.Count; i++)
                    {
                        if (name == _trunkTransforms[_redTrunksTaken[i]].name)
                        {
                            usedTrunk = i;
                            break;
                        }
                    }

                    for (int i = usedTrunk + 1; i < _redTrunksTaken.Count; i++)
                    {
                        _trunkTransforms[_redTrunksTaken[i]].localPosition += 2 * _moveToSide;
                    }

                    trunkIndexToDestroy = _redTrunksTaken[usedTrunk];
                    _redTrunksTaken.Remove(usedTrunk);
                    _redTrunks.Remove(_trunkTransforms[trunkIndexToDestroy]);

                    //Destroy(_trunkTransforms[trunkToDestroy].gameObject, 1.5f);
                    break;
                }
            case PlayerColor.GREEN:
                {
                    for (int i = 0; i < _greenTrunksTaken.Count; i++)
                    {
                        if (name == _trunkTransforms[_greenTrunksTaken[i]].name)
                        {
                            usedTrunk = i;
                            break;
                        }
                    }

                    for (int i = usedTrunk + 1; i < _greenTrunksTaken.Count; i++)
                    {
                        _trunkTransforms[_greenTrunksTaken[i]].localPosition += 2 * _moveToSide;
                    }

                    trunkIndexToDestroy = _greenTrunksTaken[usedTrunk];
                    _greenTrunksTaken.Remove(usedTrunk);
                    _greenTrunks.Remove(_trunkTransforms[trunkIndexToDestroy]);

                    //Destroy(_trunkTransforms[trunkToDestroy].gameObject, 1.5f);
                    break;
                }
            case PlayerColor.YELLOW:
                {
                    for (int i = 0; i < _yellowTrunksTaken.Count; i++)
                    {
                        if (name == _trunkTransforms[_yellowTrunksTaken[i]].name)
                        {
                            usedTrunk = i;
                            break;
                        }
                    }

                    for (int i = usedTrunk + 1; i < _yellowTrunksTaken.Count; i++)
                    {
                        _trunkTransforms[_yellowTrunksTaken[i]].localPosition += 2 * _moveToSide;
                    }

                    trunkIndexToDestroy = _yellowTrunksTaken[usedTrunk];
                    _yellowTrunksTaken.Remove(usedTrunk);
                    _yellowTrunks.Remove(_trunkTransforms[trunkIndexToDestroy]);

                    //Destroy(_trunkTransforms[trunkToDestroy].gameObject, 1.5f);
                    break;
                }
        }

        if (trunkIndexToDestroy != -1)
            _trunkToDisable = _trunkTransforms[trunkIndexToDestroy].gameObject;

        //for (int i = 0; i < _trunksTaken.Count; i++)
        //{
        //    if (name == _trunkTransforms[_trunksTaken[i]].name)
        //    {
        //        usedTrunk = i;
        //        break;
        //    }
        //}

        //for (int i = usedTrunk + 1; i < _trunksTaken.Count; i++)
        //{
        //    _trunkTransforms[_trunksTaken[i]].localPosition += 2 * _moveToSide;
        //}

        //int trunkToDestroy = _trunksTaken[usedTrunk];
        //_trunksTaken.Remove(usedTrunk);

        //Destroy(_trunkTransforms[trunkToDestroy].gameObject, 1.5f);
    }

    public void DisableUsedTrunk()
    {
        Debug.Log("DisableUsedTrunk");
        _trunkToDisable.SetActive(false);
    }

    private IEnumerator DestroyTrunk(int i)
    {
        yield return new WaitForSeconds(1.5f);

        Destroy(_trunkTransforms[i].gameObject);
    }

    public void ShowBlueTrunks()
    {
        foreach (Transform t in _yellowTrunks)
        {
            t.gameObject.SetActive(false);
        }

        foreach (Transform t in _blueTrunks)
        {
            t.gameObject.SetActive(true);
        }
    }

    public void ShowRedTrunks()
    {
        foreach (Transform t in _blueTrunks)
        {
            t.gameObject.SetActive(false);
        }

        foreach (Transform t in _redTrunks)
        {
            t.gameObject.SetActive(true);
        }
    }

    public void ShowGreenTrunks()
    {
        foreach (Transform t in _redTrunks)
        {
            t.gameObject.SetActive(false);
        }

        foreach (Transform t in _greenTrunks)
        {
            t.gameObject.SetActive(true);
        }
    }

    public void ShowYellowTrunks()
    {
        foreach (Transform t in _greenTrunks)
        {
            t.gameObject.SetActive(false);
        }

        foreach (Transform t in _yellowTrunks)
        {
            t.gameObject.SetActive(true);
        }
    }

    public void UseBombTrunk(int index)
    {
        _retreatScript.RetreatAllDice(index);
    }

    public void UseAmbushTrunk(int index)
    {
        print("USING AMBUSH TRUNK");
        //_imageAssigner.OutlineTilesWithDice();

        StartCoroutine(SwitchTileDice());
    }
    
    public void UseRollAgainTrunk(int index)
    {
        _buttonRollScript.RollDice();
        //switch (_turnManager.Current)
        //{
        //    case
        //
        //
        //    .BLUE:
        //        //GameObject.FindGameObjectsWithTag("BlueSelectable");
        //        break;
        //    case PlayerColor.RED:
        //        break;
        //    case PlayerColor.GREEN:
        //        break;
        //    case PlayerColor.YELLOW:
        //        break;
        //}
    }

    IEnumerator SwitchTileDice()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        GameObject hitGo = null;

        print("SWITCHING TILE DICE");

        while (chosenTiles < 2)
        {
            print("IN WHILE");
            if (Physics.Raycast(ray, out _raycastHit, Mathf.Infinity, _layerMask))
            {
                hitGo = _raycastHit.transform.gameObject;
            }
            if (Input.GetMouseButtonDown(0) && hitGo != null && _imagesForSwitch[chosenTiles] != hitGo)
            {
                _imagesForSwitch[chosenTiles] = _raycastHit.transform.gameObject;
                chosenTiles++;
                if (chosenTiles == 2)
                {
                    break;
                }
            }

            yield return null;
        }

        print("AFTER WHILE");

        yield return new WaitForSeconds(0.1f);

        //_imageAssigner.SwitchTileDice(_imagesForSwitch);
        _clickTileEventScript.SetAction(PlayerAction.ATTACK);
        chosenTiles = 0;
    }
}
