using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Wagon : NetworkBehaviour
{
    public int AssignedNumber;
    public Trunk AssignedTrunk;
    [SyncVar] public GameObject BlueGroup;
    [SyncVar] public GameObject RedGroup;
    [SyncVar] public GameObject YellowGroup;
    [SyncVar] public GameObject GreenGroup;
    [SyncVar] public int BlueNumber = 0;
    [SyncVar] public int GreenNumber = 0;
    [SyncVar] public int YellowNumber = 0;
    [SyncVar] public int RedNumber = 0;
    public bool Last;
    [SerializeField] private PlayerColor _currentPlayer;
    [SerializeField, FormerlySerializedAs("_currentPlayerScript")] private TurnManager _turnManager;

    public bool CanSteal(PlayerColor player)
    {
        Debug.Log("AssignedNumber: " + AssignedNumber);

        //if (player == PlayerColor.BLUE)
        //    return BlueNumber >= AssignedNumber && BlueNumber > RedNumber && BlueNumber > GreenNumber && BlueNumber > YellowNumber;
        //else if (player == PlayerColor.RED)
        //    return RedNumber >= AssignedNumber;
        //else if (player == PlayerColor.GREEN)
        //    return GreenNumber >= AssignedNumber;
        //else if (player == PlayerColor.YELLOW)
        //    return YellowNumber >= AssignedNumber;
        //else
        //    return false;

        int[] numbers = new int[4] { BlueNumber, RedNumber, GreenNumber, YellowNumber };
        int currentIndex = (int)player;
        int currentPlayerNumber = numbers[currentIndex];

        if (currentPlayerNumber < AssignedNumber)
            return false;

        for (int i = 0; i < numbers.Length; i++)
        {
            if (i != currentIndex && numbers[i] >= currentPlayerNumber)
                return false;
        }

        return true;
    }

    public bool HasDice()
    {
        _currentPlayer = _turnManager.Current;
        if (_currentPlayer == PlayerColor.BLUE && BlueNumber > 0)
        {
            //print("Has blueNumber > 0");
            return true;
        }
        else if (_currentPlayer == PlayerColor.RED && RedNumber > 0)
        {
            //print("Has redNumber > 0");
            return true;
        }
        else if (_currentPlayer == PlayerColor.GREEN && GreenNumber > 0)
        {
            //print("Has greenNumber > 0");
            return true;
        }
        else if (_currentPlayer == PlayerColor.YELLOW && YellowNumber > 0)
        {
            //print("Has yellowNumber > 0");
            return true;
        }
        else
        {
            //print("Has no number");
            return false;
        }
    }

    public bool CanTakeTrunk()
    {
        _currentPlayer = _turnManager.Current;
        switch (_currentPlayer)
        {
            case PlayerColor.BLUE:
                return BlueNumber >= 3;
            case PlayerColor.RED:
                return RedNumber >= 3;
            case PlayerColor.GREEN:
                return GreenNumber >= 3;
            case PlayerColor.YELLOW:
                return YellowNumber >= 3;
        }

        return false;
    }

    public string GetCurrentPlayerDiceName()
    {
        switch (_currentPlayer)
        {
            case PlayerColor.BLUE:
                return BlueGroup.name;
            case PlayerColor.RED:
                return RedGroup.name;
            case PlayerColor.GREEN:
                return GreenGroup.name;
            case PlayerColor.YELLOW:
                return YellowGroup.name;
        }

        return null;
    }

    public void ResetAfterSteal()
    {
        BlueGroup = null;
        RedGroup = null;
        GreenGroup = null;
        YellowGroup = null;

        BlueNumber = 0;
        RedNumber = 0;
        GreenNumber = 0;
        YellowNumber = 0;
    }

    [ClientRpc]
    public void RpcResetAfterSteal()
    {
        ResetAfterSteal();
    }
}
