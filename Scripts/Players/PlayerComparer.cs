using System.Collections.Generic;

public class PlayerComparer : IComparer<Player>
{
    public int Compare(Player x, Player y)
    {
        // First level: Compare by Points
        int pointsComparison = y.Points.CompareTo(x.Points);
        if (pointsComparison != 0)
        {
            return pointsComparison;
        }

        // Second level: If Points are equal, compare by WagonsStolen
        int wagonsStolenComparison = y.WagonsStolen.CompareTo(x.WagonsStolen);
        if (wagonsStolenComparison != 0)
        {
            return wagonsStolenComparison;
        }

        // Third level: If WagonsStolen are equal, compare by TrunksTaken
        return y.TrunksTaken.CompareTo(x.TrunksTaken);
    }
}