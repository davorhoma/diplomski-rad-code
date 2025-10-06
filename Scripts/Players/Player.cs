using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public PlayerColor Color { get; set; }
    public string Name { get; set; }
    public int Points { get; set; }
    public int TrunksTaken { get; set; }
    public int TrunkPoints { get; set; }
    public int WagonsStolen { get; set; }
    public int WagonPoints { get; set; }
    public int TrunksInHand { get; set; }

    public Player() { }

    public Player(PlayerColor color, string name, int points, int trunksTaken, int wagonsStolen)
    {
        Color = color;
        Name = name;
        Points = points;
        TrunksTaken = trunksTaken;
        WagonsStolen = wagonsStolen;
    }

    public int GetTotalPoints()
    {
        return WagonPoints + TrunkPoints;
    }

    public void AddWagonPoints(int points)
    {
        WagonPoints += points;
        WagonsStolen++;
    }

    public void AddTrunk()
    {

    }

    public void RemoveTrunk()
    {

    }
}
