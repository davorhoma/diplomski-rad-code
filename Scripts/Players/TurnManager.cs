using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    [SyncVar(hook = nameof(OnCurrentPlayerChanged))] private PlayerColor _currentPlayer;
    public PlayerColor Current => _currentPlayer;
    private int _numberOfPlayers;
    public int NumberOfPlayers => _numberOfPlayers;

    [SerializeField] TMP_Text screenText;
    private ButtonRoll _buttonRollScript;
    private SelectOutline _selectOutlineScript;
    [SerializeField] private TrunkPlacer _trunkPlacerScript;
    private ClickTileEvent _clickTileEventScript;

    [SerializeField, FormerlySerializedAs("_applyForceMonoBlue")] private DiceActionManager _diceActionManagerBlue;
    [SerializeField, FormerlySerializedAs("_applyForceMonoRed")] private DiceActionManager _diceActionManagerRed;
    [SerializeField, FormerlySerializedAs("_applyForceMonoGreen")] private DiceActionManager _diceActionManagerGreen;
    [SerializeField, FormerlySerializedAs("_applyForceMonoYellow")] private DiceActionManager _diceActionManagerYellow;

    private Button _buttonEndTurn;
    private Button _buttonRoll;
    private Button _buttonSteal;
    private Button _buttonRetreat;

    [SerializeField] private Transform _blueDiceStart;
    [SerializeField] private Transform _redDiceStart;
    [SerializeField] private Transform _greenDiceStart;
    [SerializeField] private Transform _yellowDiceStart;

    private PlayerObjectController _localPlayer;

    //Manager
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

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        _currentPlayer = PlayerColor.BLUE;
        _buttonRollScript = GetComponent<ButtonRoll>();
        _selectOutlineScript = GetComponent<SelectOutline>();
        _clickTileEventScript = GetComponent<ClickTileEvent>();

        _numberOfPlayers = Manager.GamePlayers.Count;
    }

    public override void OnStartClient()
    {
        if (isClient)
        {
            _localPlayer = GameObject.Find("LocalGamePlayer").GetComponent<PlayerObjectController>();
            //Debug.Log("Local player object name: " + _localPlayer.name);
            //Debug.Log("Hiiii" + _localPlayer.PlayerColor);

            _buttonRoll = GameObject.Find("Button Roll").GetComponent<Button>();
            _buttonEndTurn = GameObject.Find("Button End Turn").GetComponent<Button>();
            _buttonSteal = GameObject.Find("Button Steal").GetComponent<Button>();
            _buttonRetreat = GameObject.Find("Button Retreat").GetComponent<Button>();

            //Debug.Log("Updated button ineractivity");
            UpdateButtonInteractivity();
            UpdateScriptsActivity();
        }
    }

    public void NextPlayer()
    {
        _buttonEndTurn.interactable = false;
        _buttonRoll.interactable = true;
        _buttonSteal.interactable = true;
        _buttonRetreat.interactable = true;

        _selectOutlineScript.enabled = false;
        _clickTileEventScript.enabled = false;

        //Debug.Log("Next player");
        if (NetworkClient.localPlayer != null)
        {
            var script = NetworkClient.localPlayer.GetComponent<PlayerCommandController>();
            script.CmdNextPlayer();

            //Debug.Log("Local player");
        }

    }

    [Server]
    public IEnumerator NextPlayerCoroutine()
    {
        yield return StartCoroutine(TimeUtil.Wait(0.2f));
        _clickTileEventScript.SetAction(PlayerAction.NEUTRAL);

        switch (_numberOfPlayers)
        {
            case 1:
                yield return StartCoroutine(_diceActionManagerBlue.SetDiceInCorner(_blueDiceStart.position));
                _selectOutlineScript.SetSelectionTag("BluePositioned");
                RpcSetScreenText("Blue plays", Color.blue);
                //screenText.text = "Blue plays";
                //screenText.color = Color.blue;
                _currentPlayer = PlayerColor.BLUE;
                _trunkPlacerScript.ShowBlueTrunks();
                break;
            case 2:
                if (_currentPlayer == PlayerColor.BLUE)
                {
                    Debug.Log("Player color was: " + _currentPlayer);
                    yield return StartCoroutine(_diceActionManagerBlue.SetDiceInCorner(_blueDiceStart.position));
                    _selectOutlineScript.SetSelectionTag("RedPositioned");
                    RpcSetScreenText("Red plays", Color.red);
                    //screenText.text = "Red plays";
                    //screenText.color = Color.red;
                    _currentPlayer = PlayerColor.RED;

                    _trunkPlacerScript.ShowRedTrunks();

                    Debug.Log("Player color now: " + _currentPlayer);
                }
                else
                {
                    Debug.Log("Player color was: " + _currentPlayer);
                    yield return StartCoroutine(_diceActionManagerRed.SetDiceInCorner(_redDiceStart.position));
                    _selectOutlineScript.SetSelectionTag("BluePositioned");
                    RpcSetScreenText("Blue plays", Color.blue);
                    //screenText.text = "Blue plays";
                    //screenText.color = Color.blue;
                    _currentPlayer = PlayerColor.BLUE;

                    _trunkPlacerScript.ShowBlueTrunks();
                    Debug.Log("Player color now: " + _currentPlayer);
                }
                break;
            case 3:
                if (_currentPlayer == PlayerColor.BLUE)
                {
                    yield return StartCoroutine(_diceActionManagerBlue.SetDiceInCorner(_blueDiceStart.position));
                    _selectOutlineScript.SetSelectionTag("RedPositioned");
                    RpcSetScreenText("Red plays", Color.red);
                    //screenText.text = "Red plays";
                    //screenText.color = Color.red;
                    _currentPlayer = PlayerColor.RED;

                    _trunkPlacerScript.ShowRedTrunks();
                }
                else if (_currentPlayer == PlayerColor.RED)
                {
                    yield return StartCoroutine(_diceActionManagerRed.SetDiceInCorner(_redDiceStart.position));
                    _selectOutlineScript.SetSelectionTag("GreenPositioned");
                    RpcSetScreenText("Green plays", Color.green);
                    //screenText.text = "Green plays";
                    //screenText.color = Color.green;
                    _currentPlayer = PlayerColor.GREEN;

                    _trunkPlacerScript.ShowGreenTrunks();
                }
                else
                {
                    yield return StartCoroutine(_diceActionManagerGreen.SetDiceInCorner(_greenDiceStart.position));
                    _selectOutlineScript.SetSelectionTag("BluePositioned");
                    RpcSetScreenText("Blue plays", Color.blue);
                    //screenText.text = "Blue plays";
                    //screenText.color = Color.blue;
                    _currentPlayer = PlayerColor.BLUE;

                    _trunkPlacerScript.ShowBlueTrunks();
                }
                break;
            case 4:
                if (_currentPlayer == PlayerColor.BLUE)
                {
                    yield return StartCoroutine(_diceActionManagerBlue.SetDiceInCorner(_blueDiceStart.position));
                    _selectOutlineScript.SetSelectionTag("RedPositioned");
                    RpcSetScreenText("Red plays", Color.red);
                    //screenText.text = "Red plays";
                    //screenText.color = Color.red;
                    _currentPlayer = PlayerColor.RED;

                    _trunkPlacerScript.ShowRedTrunks();
                }
                else if (_currentPlayer == PlayerColor.RED)
                {
                    yield return StartCoroutine(_diceActionManagerRed.SetDiceInCorner(_redDiceStart.position));
                    _selectOutlineScript.SetSelectionTag("GreenPositioned");
                    RpcSetScreenText("Green plays", Color.green);
                    //screenText.text = "Green plays";
                    //screenText.color = Color.green;
                    _currentPlayer = PlayerColor.GREEN;

                    _trunkPlacerScript.ShowGreenTrunks();
                }
                else if (_currentPlayer == PlayerColor.GREEN)
                {
                    yield return StartCoroutine(_diceActionManagerGreen.SetDiceInCorner(_greenDiceStart.position));
                    _selectOutlineScript.SetSelectionTag("YellowPositioned");
                    RpcSetScreenText("Yellow plays", Color.yellow);
                    //screenText.text = "Yellow plays";
                    //screenText.color = Color.yellow;
                    _currentPlayer = PlayerColor.YELLOW;

                    _trunkPlacerScript.ShowYellowTrunks();
                }
                else
                {
                    yield return StartCoroutine(_diceActionManagerYellow.SetDiceInCorner(_yellowDiceStart.position));
                    _selectOutlineScript.SetSelectionTag("BluePositioned");
                    RpcSetScreenText("Blue plays", Color.blue);
                    //screenText.text = "Blue plays";
                    //screenText.color = Color.blue;
                    _currentPlayer = PlayerColor.BLUE;

                    _trunkPlacerScript.ShowBlueTrunks();
                }
                break;
            default:
                Debug.Log("Invalid number of players");
                break;
        }

        //yield return StartCoroutine(Show());
        //RpcShowScreenText();
        RpcUpdateButtonInteractivity();
        RpcUpdateScriptsActivity();
    }

    private IEnumerator Wait(float time = 1f)
    {
        yield return new WaitForSeconds(time);
    }

    private IEnumerator Show()
    {
        screenText.enabled = true;
        //screenText.CrossFadeAlpha(0, 1f, true);
        yield return new WaitForSeconds(1f);
        screenText.enabled = false;
    }

    [ClientRpc]
    private void RpcShowScreenText()
    {
        Debug.Log("Rpc show screen text");
        StartCoroutine(Show());
    }

    [ClientRpc]
    private void RpcSetScreenText(string text, Color color)
    {
        screenText.text = text;
        screenText.color = color;
        StartCoroutine(Show());
    }

    public string SetTagSelectable(GameObject go)
    {
        if (_currentPlayer == PlayerColor.BLUE)
        {
            go.tag = "BlueSelectable";
        }
        else if (_currentPlayer == PlayerColor.RED)
        {
            go.tag = "RedSelectable";
        }
        else if (_currentPlayer == PlayerColor.GREEN)
        {
            go.tag = "GreenSelectable";
        }
        else
        {
            go.tag = "YellowSelectable";
        }

        return go.tag;
    }

    public void SetTagPositioned(GameObject go)
    {
        if (_currentPlayer == PlayerColor.BLUE)
        {
            go.tag = "BluePositioned";
        }
        else if (_currentPlayer == PlayerColor.RED)
        {
            go.tag = "RedPositioned";
        }
        else if (_currentPlayer == PlayerColor.GREEN)
        {
            go.tag = "GreenPositioned";
        }
        else
        {
            go.tag = "YellowPositioned";
        }
    }

    public void SetSelectionTag()
    {
        if (_currentPlayer == PlayerColor.BLUE)
        {
            _selectOutlineScript.SetSelectionTag("BlueSelectable");
        }
        else if (_currentPlayer == PlayerColor.RED)
        {
            _selectOutlineScript.SetSelectionTag("RedSelectable");
        }
        else if (_currentPlayer == PlayerColor.GREEN)
        {
            _selectOutlineScript.SetSelectionTag("GreenSelectable");
        }
        else
        {
            _selectOutlineScript.SetSelectionTag("YellowSelectable");
        }
    }

    [ClientRpc]
    public void RpcSetTagPositioned(uint netId)
    {
        if (!NetworkClient.spawned.TryGetValue(netId, out var networkIdentity))
        {
            return;
        }

        SetTagPositioned(networkIdentity.gameObject);
    }

    [Client]
    private void UpdateButtonInteractivity()
    {
        bool isLocalPlayerTurn = false;
        if (_localPlayer != null)
        {
            isLocalPlayerTurn = _localPlayer.PlayerColor == _currentPlayer;
            //Debug.Log("Color: " + _localPlayer.PlayerColor);
            //Debug.Log("Current player color: " + _currentPlayer);
        }

        _buttonEndTurn.interactable = isLocalPlayerTurn;
        _buttonRoll.interactable = isLocalPlayerTurn;
        _buttonSteal.interactable = isLocalPlayerTurn;
        _buttonRetreat.interactable = isLocalPlayerTurn;
    }

    [Client]
    private void UpdateScriptsActivity()
    {
        Debug.Log("UpdateScriptsActivity");
        bool isLocalPlayerTurn = false;
        if (_localPlayer != null)
        {
            isLocalPlayerTurn = _localPlayer.PlayerColor == _currentPlayer;
        }

        _clickTileEventScript.enabled = isLocalPlayerTurn;
        _selectOutlineScript.enabled = isLocalPlayerTurn;
        Debug.Log("_selectOutlineScript.enabled: " +  _selectOutlineScript.enabled);
        Debug.Log("_clickTileEventScript.enabled: " +  _clickTileEventScript.enabled);
    }

    [ClientRpc]
    private void RpcUpdateButtonInteractivity()
    {
        UpdateButtonInteractivity();
    }

    [ClientRpc]
    public void RpcUpdateScriptsActivity()
    {
        Debug.Log("RpcUpdateScriptsActivity");
        UpdateScriptsActivity();
    }

    private void OnCurrentPlayerChanged(PlayerColor oldValue, PlayerColor newValue)
    {
        if (_localPlayer != null) UpdateButtonInteractivity();
    }

    public string GetCurrentSelectableTag()
    {
        if (_currentPlayer == PlayerColor.BLUE)
        {
            return "BlueSelectable";
        }
        else if (_currentPlayer == PlayerColor.RED)
        {
            return "RedSelectable";
        }
        else if (_currentPlayer == PlayerColor.GREEN)
        {
            return "GreenSelectable";
        }
        else
        {
            return "YellowSelectable";
        }
    }
}
