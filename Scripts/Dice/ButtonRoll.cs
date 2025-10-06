using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ButtonRoll : NetworkBehaviour
{
    public static ButtonRoll Instance;

    [SerializeField] float _rollForce = 15f;
    [SerializeField] float _torqueAmount = 100f;
    [SerializeField] LayerMask _layerMask;

    public bool _rolling;
    public Transform _transform;

    private SelectOutline _selectOutlineScript;
    private ClickTileEvent _clickTileScript;
    //private Players _playersScript;
    private TurnManager _turnManager;
    private PlayerColor _currentPlayer;

    [SerializeField] private Image[] _images;
    [SerializeField] private ImageAssigner _imageAssigner;
    [SerializeField, FormerlySerializedAs("_imageProperties")] private Wagon[] _wagons;
    [SerializeField] private Retreat _retreatScript;

    private List<int> _retreatedDiceIndexes = new List<int>();
    //public Image Image
    //{
    //    get { return _image; }
    //    set { _image = value; }
    //}
    //private Material material;
    //private GameObject _diceObject;

    [SyncVar] public GameObject BluePlayer;
    [SyncVar] public GameObject RedPlayer;
    [SyncVar] public GameObject GreenPlayer;
    [SyncVar] public GameObject YellowPlayer;

    //private string selectableTag = "Selectable";

    [SerializeField] private ApplyForce _applyForceBlue;
    [SerializeField] private ApplyForce _applyForceRed;
    [SerializeField] private ApplyForce _applyForceGreen;
    [SerializeField] private ApplyForce _applyForceYellow;

    [SerializeField, FormerlySerializedAs("_applyForceMonoBlue")] private DiceActionManager _diceActionManagerBlue;
    [SerializeField, FormerlySerializedAs("_applyForceMonoRed")] private DiceActionManager _diceActionManagerRed;
    [SerializeField, FormerlySerializedAs("_applyForceMonoGreen")] private DiceActionManager _diceActionManagerGreen;
    [SerializeField, FormerlySerializedAs("_applyForceMonoYellow")] private DiceActionManager _diceActionManagerYellow;

    [SerializeField] private Button _buttonRoll;
    [SerializeField] private Button _buttonSteal;
    [SerializeField] private Button _buttonRetreat;

    [SerializeField] private GameObject _rollingPad;

    private List<GameDie> allGameDice = new List<GameDie>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; }

        //_playersScript = GetComponent<Players>();
        _selectOutlineScript = GetComponent<SelectOutline>();
        _clickTileScript = GetComponent<ClickTileEvent>();
        _turnManager = GetComponent<TurnManager>();

        var allDiceRigidbodies = GameObject.FindObjectsOfType<Rigidbody>();
        foreach (var rb in allDiceRigidbodies)
        {
            allGameDice.Add(rb.GetComponent<GameDie>());
        }
    }

    private void Update()
    {
        
    }

    public void RollDice()
    {
        _selectOutlineScript.enabled = false;
        _clickTileScript.enabled = false;

        _buttonRoll.interactable = false;
        _buttonSteal.interactable = false;
        _buttonRetreat.interactable = false;

        _rollingPad.SetActive(true);

        DisableDiceOutlineIfActive();

        if (NetworkClient.localPlayer != null)
        {
            var script = NetworkClient.localPlayer.GetComponent<PlayerCommandController>();
            script.CmdRollDice();

            //CmdRollDice();
        }
    }

    private void DisableDiceOutlineIfActive()
    {
        _currentPlayer = _turnManager.Current;
        if (_retreatScript.enabled == true)
        {
            if (_currentPlayer == PlayerColor.BLUE)
            {
                foreach (int i in _retreatedDiceIndexes)
                {
                    if (_wagons[i].BlueGroup)
                        _selectOutlineScript.DisableOutline(_wagons[i].BlueGroup);
                }
            }
            else if (_currentPlayer == PlayerColor.RED)
            {
                foreach (int i in _retreatedDiceIndexes)
                {
                    if (_wagons[i].RedGroup)
                        _selectOutlineScript.DisableOutline(_wagons[i].RedGroup);
                }
            }
            else if (_currentPlayer == PlayerColor.GREEN)
            {
                foreach (int i in _retreatedDiceIndexes)
                {
                    if (_wagons[i].GreenGroup)
                        _selectOutlineScript.DisableOutline(_wagons[i].GreenGroup);
                }
            }
            else
            {
                foreach (int i in _retreatedDiceIndexes)
                {
                    if (_wagons[i].YellowGroup)
                        _selectOutlineScript.DisableOutline(_wagons[i].YellowGroup);
                }
            }

            _retreatScript.enabled = false;
        }
    }

    [Server]
    public IEnumerator ServerRollDice()
    {
        PlayerActionManager.Instance.SetAction(PlayerAction.ROLL);

        //_selectOutlineScript.enabled = false;
        //_clickTileScript.enabled = false;

        //_buttonRoll.interactable = false;
        //_buttonSteal.interactable = false;
        //_buttonRetreat.interactable = false;

        _currentPlayer = _turnManager.Current;

        _rolling = true;

        yield return new WaitForSeconds(0.5f);
        while (!AreAllDiceSleeping())
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (_currentPlayer == PlayerColor.BLUE)
        {
            print("Applying force to blue");
            yield return _diceActionManagerBlue.RollDice(_rollForce, _torqueAmount);
            yield return WaitForDiceToStop(_diceActionManagerBlue);
        }
        else if (_currentPlayer == PlayerColor.RED)
        {
            print("Applying force to red");
            yield return _diceActionManagerRed.RollDice(_rollForce, _torqueAmount);
            yield return WaitForDiceToStop(_diceActionManagerRed);
        }
        else if (_currentPlayer == PlayerColor.GREEN)
        {
            print("Applying force to green");
            yield return _diceActionManagerGreen.RollDice(_rollForce, _torqueAmount);
            yield return WaitForDiceToStop(_diceActionManagerGreen);
        }
        else
        {
            print("Applying force to yellow");
            yield return _diceActionManagerYellow.RollDice(_rollForce, _torqueAmount);
            yield return WaitForDiceToStop(_diceActionManagerYellow);
        }
        
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator WaitForDiceToStop(DiceActionManager applyForceMono)
    {
        while (!applyForceMono.AreAllDiceSleepingAndHaveValue())
        {
            yield return null;
        }
    }

    [TargetRpc]
    public void TargetRetreatDice(NetworkConnectionToClient conn, uint netId, int i)
    {
        if (!NetworkClient.spawned.TryGetValue(netId, out var networkIdentity))
        {
            Debug.Log("Cannot find selected group by netId");
            return;
        }

        PlayerActionManager.Instance.SetAction(PlayerAction.RETREAT);
        _selectOutlineScript.enabled = false;

        _retreatedDiceIndexes.Add(i);
        _selectOutlineScript.EnableOutline(networkIdentity.gameObject);

        _retreatScript.enabled = true;
        _rollingPad.SetActive(false);
    }

    public void RetreatDice()
    {
        PlayerActionManager.Instance.SetAction(PlayerAction.RETREAT);
        PlayerColor currentPlayer = _turnManager.Current;
        _selectOutlineScript.enabled = false;

        for (int i = 0; i < _wagons.Length; i++)
        {
            if (_wagons[i].HasDice())
            {
                _retreatedDiceIndexes.Add(i);
                switch (currentPlayer)
                {
                    case PlayerColor.BLUE:
                        _selectOutlineScript.EnableOutline(_wagons[i].BlueGroup);
                        break;
                    case PlayerColor.RED:
                        _selectOutlineScript.EnableOutline(_wagons[i].RedGroup);
                        break;
                    case PlayerColor.GREEN:
                        _selectOutlineScript.EnableOutline(_wagons[i].GreenGroup);
                        break;
                    case PlayerColor.YELLOW:
                        _selectOutlineScript.EnableOutline(_wagons[i].YellowGroup);
                        break;
                }

                break;
            }
        }

        _retreatScript.enabled = true;
        _rollingPad.SetActive(false);
    }

    private bool AreAllDiceSleeping()
    {
        foreach (var gameDie in allGameDice)
        {
            if (!gameDie.IsDieSleeping())
            {
                Debug.Log("Not sleeping");
                return false;
            }
        }

        Debug.Log("All dice are sleeping");
        return true;
    }
}
