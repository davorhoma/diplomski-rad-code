using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public enum PlayerColor { BLUE = 0, RED = 1, GREEN = 2, YELLOW = 3 };

public class PointCounter : NetworkBehaviour
{
    private Player _bluePlayer;
    private Player _redPlayer;
    private Player _greenPlayer;
    private Player _yellowPlayer;

    [SyncVar(hook = nameof(OnPointsChange))] private int _bluePoints;
    [SyncVar(hook = nameof(OnPointsChange))] private int _redPoints;
    [SyncVar(hook = nameof(OnPointsChange))] private int _greenPoints;
    [SyncVar(hook = nameof(OnPointsChange))] private int _yellowPoints;

    [SyncVar(hook = nameof(OnWagonPointsChange))] private int _blueWagonPoints;
    [SyncVar(hook = nameof(OnWagonPointsChange))] private int _redWagonPoints;
    [SyncVar(hook = nameof(OnWagonPointsChange))] private int _greenWagonPoints;
    [SyncVar(hook = nameof(OnWagonPointsChange))] private int _yellowWagonPoints;

    [SyncVar(hook = nameof(OnTrunkPointsChange))] private int _blueTrunkPoints;
    [SyncVar(hook = nameof(OnTrunkPointsChange))] private int _redTrunkPoints;
    [SyncVar(hook = nameof(OnTrunkPointsChange))] private int _greenTrunkPoints;
    [SyncVar(hook = nameof(OnTrunkPointsChange))] private int _yellowTrunkPoints;

    [SyncVar(hook = nameof(OnTrunkInHandChange))] private int _blueTrunksInHand;
    [SyncVar(hook = nameof(OnTrunkInHandChange))] private int _redTrunksInHand;
    [SyncVar(hook = nameof(OnTrunkInHandChange))] private int _greenTrunksInHand;
    [SyncVar(hook = nameof(OnTrunkInHandChange))] private int _yellowTrunksInHand;

    public int BluePoints => _bluePoints;
    public int RedPoints => _redPoints;
    public int GreenPoints => _greenPoints;
    public int YellowPoints => _yellowPoints;

    public int BlueWagonPoints => _blueWagonPoints;
    public int RedWagonPoints => _redWagonPoints;
    public int GreenWagonPoints => _greenWagonPoints;
    public int YellowWagonPoints => _yellowWagonPoints;

    public int BlueTrunkPoints => _blueTrunkPoints;
    public int RedTrunkPoints => _redTrunkPoints;
    public int GreenTrunkPoints => _greenTrunkPoints;
    public int YellowTrunkPoints => _yellowTrunkPoints;

    public int BlueTrunksInHand => _blueTrunksInHand;
    public int RedTrunksInHand => _redTrunksInHand;
    public int GreenTrunksInHand => _greenTrunksInHand;
    public int YellowTrunksInHand => _yellowTrunksInHand;
    //[SerializeField] private int _blueTrunkPoints;
    //private int _blueWagons;

    //[SerializeField] private int _redTrunkPoints;
    //private int _redWagons;

    //[SerializeField] private int _greenTrunkPoints;
    //private int _greenWagons;

    //[SerializeField] private int _yellowTrunkPoints;
    //private int _yellowWagons;

    [SerializeField] private TMP_Text _blue;
    [SerializeField] private TMP_Text _blueTrunk;

    [SerializeField] private TMP_Text _red;
    [SerializeField] private TMP_Text _redTrunk;

    [SerializeField] private TMP_Text _green;
    [SerializeField] private TMP_Text _greenTrunk;

    [SerializeField] private TMP_Text _yellow;
    [SerializeField] private TMP_Text _yellowTrunk;

    [SerializeField, FormerlySerializedAs("_currentPlayerScript")] private TurnManager _turnManager;
    [SerializeField] private ScoreboardMenu _scoreboardMenuScript;
 
    void Start()
    {
        _bluePlayer = new Player(PlayerColor.BLUE, "Player1", 0, 0, 0);
        _redPlayer = new Player(PlayerColor.RED, "Player2", 0, 0, 0);
        _greenPlayer = new Player(PlayerColor.GREEN, "Player3", 0, 0, 0);
        _yellowPlayer = new Player(PlayerColor.YELLOW, "Player4", 0, 0, 0);

        _blue.text = _bluePlayer.Points.ToString();
        _red.text = _redPlayer.Points.ToString();
        _green.text = _greenPlayer.Points.ToString();
        _yellow.text = _yellowPlayer.Points.ToString();

        _blueTrunk.text = _bluePlayer.TrunksTaken.ToString();
        _redTrunk.text = _redPlayer.TrunksTaken.ToString();
        _greenTrunk.text = _greenPlayer.TrunksTaken.ToString();
        _yellowTrunk.text = _yellowPlayer.TrunksTaken.ToString();
    }

    public void AddWagonPoints(int points)
    {
        switch (_turnManager.Current)
        {
            case PlayerColor.BLUE:
                _bluePoints += points;
                _bluePlayer.Points += points;
                _bluePlayer.WagonsStolen++;
                _bluePlayer.WagonPoints += points;
                _blueWagonPoints += points;
                //_blue.text = _bluePoints.ToString();
                break;
            case PlayerColor.RED:
                _redPoints += points;
                _redPlayer.Points += points;
                _redPlayer.WagonsStolen++;
                _redPlayer.WagonPoints += points;
                _redWagonPoints += points;
                //_red.text = _redPoints.ToString();
                break;
            case PlayerColor.GREEN:
                _greenPoints += points;
                _greenPlayer.Points += points;
                _greenPlayer.WagonsStolen++;
                _greenPlayer.WagonPoints += points;
                _greenWagonPoints += points;
                //_green.text = _greenPoints.ToString();
                break;
            case PlayerColor.YELLOW:
                _yellowPoints += points;
                _yellowPlayer.Points += points;
                _yellowPlayer.WagonsStolen++;
                _yellowPlayer.WagonPoints += points;
                _yellowWagonPoints += points;
                //_yellow.text = _yellowPoints.ToString();
                break;
        }
    }

    private void OnPointsChange(int oldValue, int newValue)
    {
        Debug.Log("RpcUpdatePoints, blue: " + _bluePoints + ", red: " + _redPoints
            + ", green: " + _greenPoints + ", yellow: " + _yellowPoints);
        _blue.text = _bluePoints.ToString();
        _red.text = _redPoints.ToString();
        _green.text = _greenPoints.ToString();
        _yellow.text = _yellowPoints.ToString();

        _scoreboardMenuScript.UpdateTotalPoints();
    }

    private void OnWagonPointsChange(int oldValue, int newValue)
    {
        _scoreboardMenuScript.UpdateWagonPoints();
    }

    private void OnTrunkPointsChange(int oldValue, int newValue)
    {
        _scoreboardMenuScript.UpdateTrunkPoints();
    }

    private void OnTrunkInHandChange(int oldValue, int newValue)
    {
        Debug.Log("RpcUpdatePoints, blue: " + _blueTrunksInHand + ", red: " + _redTrunksInHand
            + ", green: " + _greenTrunksInHand + ", yellow: " + _yellowTrunksInHand);
        _blueTrunk.text = _blueTrunksInHand.ToString();
        _redTrunk.text = _redTrunksInHand.ToString();
        _greenTrunk.text = _greenTrunksInHand.ToString();
        _yellowTrunk.text = _yellowTrunksInHand.ToString();

        _scoreboardMenuScript.UpdateTrunksInHand();
    }

    [Server]
    public void AddTrunkPoints(int points)
    {
        switch (_turnManager.Current)
        {
            case PlayerColor.BLUE:
                _bluePoints += points;
                _bluePlayer.Points += points;
                _blueTrunkPoints += points;
                _bluePlayer.TrunkPoints += points;
                //_blue.text = _bluePoints.ToString();
                break;
            case PlayerColor.RED:
                _redPoints += points;
                _redPlayer.Points += points;
                _redTrunkPoints += points;
                _redPlayer.TrunkPoints += points;
                //_red.text = _redPoints.ToString();
                break;
            case PlayerColor.GREEN:
                _greenPoints += points;
                _greenPlayer.Points += points;
                _greenTrunkPoints += points;
                _greenPlayer.TrunkPoints += points;
                //_green.text = _greenPoints.ToString();
                break;
            case PlayerColor.YELLOW:
                _yellowPoints += points;
                _yellowPlayer.Points += points;
                _yellowTrunkPoints += points;
                _yellowPlayer.TrunkPoints += points;
                //_yellow.text = _yellowPoints.ToString();
                break;
        }
    }

    public void AddTrunk()
    {
        switch (_turnManager.Current)
        {
            case PlayerColor.BLUE:
                _bluePlayer.TrunksTaken++;
                _blueTrunk.text = _bluePlayer.TrunksTaken.ToString();
                break;
            case PlayerColor.RED:
                _redPlayer.TrunksTaken++;
                _redTrunk.text = _redPlayer.TrunksTaken.ToString();
                break;
            case PlayerColor.GREEN:
                _greenPlayer.TrunksTaken++;
                _greenTrunk.text = _greenPlayer.TrunksTaken.ToString();
                break;
            case PlayerColor.YELLOW:
                _yellowPlayer.TrunksTaken++;
                _yellowTrunk.text = _yellowPlayer.TrunksTaken.ToString();
                break;
        }
    }

    public List<Player> GetSortedPlayers()
    {
        List<Player> players = new List<Player>
        {
            _bluePlayer,
            _redPlayer,
            _greenPlayer,
            _yellowPlayer
        };

        // Sort the players using the custom comparer
        players.Sort(new PlayerComparer());

        // Display the result
        //foreach (var player in players.Select((p, i) => new { Player = p, Position = i + 1 }))
        //{
        //    Console.WriteLine($"Position {player.Position}: {player.Player.Name} with {player.Player.Points} points, {player.Player.TrunksTaken} trunkPoints, and {player.Player.WagonsStolen} wagons");
        //}


        return players;
    }

    [Server]
    public void UpdateTrunkAmount(int amount)
    {
        switch (_turnManager.Current)
        {
            case PlayerColor.BLUE:
                _blueTrunksInHand += amount;
                break;
            case PlayerColor.RED:
                _redTrunksInHand += amount;
                break;
            case PlayerColor.GREEN:
                _greenTrunksInHand += amount;
                break;
            case PlayerColor.YELLOW:
                _yellowTrunksInHand += amount;
                break;
        }
    }

}
