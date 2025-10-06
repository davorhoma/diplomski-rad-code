using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerAction { NEUTRAL = 0, ATTACK = 1, STEAL = 2, RETREAT = 3, ROLL = 4, AMBUSH = 5 };
public class PlayerActionManager : NetworkBehaviour
{
    public static PlayerActionManager Instance;

    // Start is called before the first frame update
    [SyncVar] private PlayerAction action;

    [SerializeField] private Button _retreatButton;
    [SerializeField] private Button _stealButton;
    [SerializeField] private Button _rollButton;
    [SerializeField] private Button _endTurnButton;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    void Start()
    {
        action = PlayerAction.NEUTRAL;
    }

    public void SetAction(PlayerAction a)
    {
        action = a;

        switch (a)
        {
            case PlayerAction.NEUTRAL:
                _retreatButton.interactable = true;
                _stealButton.interactable = true;
                _rollButton.interactable = true;
                //_endTurnButton.interactable = true;
                break;
            case PlayerAction.ATTACK:
                _retreatButton.interactable = false;
                _stealButton.interactable = false;
                _rollButton.interactable = false;

                _endTurnButton.interactable = true;
                break;
            case PlayerAction.STEAL:
                _retreatButton.interactable = false;
                _stealButton.interactable = false;
                _endTurnButton.interactable = false;

                _rollButton.interactable = true;
                break;
            case PlayerAction.RETREAT:
                _retreatButton.interactable = false;
                _stealButton.interactable = false;
                _endTurnButton.interactable = false;

                _rollButton.interactable = true;
                break;
            case PlayerAction.ROLL:
                _retreatButton.interactable = false;
                _stealButton.interactable = false;
                _rollButton.interactable = false;

                _endTurnButton.interactable = true;
                break;
            case PlayerAction.AMBUSH:
                break;
        }
    }

    public PlayerAction GetAction() { return action; }
}
