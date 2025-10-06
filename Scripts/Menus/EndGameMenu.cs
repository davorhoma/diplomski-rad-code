using System.Collections;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class EndGameMenu : MonoBehaviour
{
    [SerializeField] private PointCounter _pointCounterScript;
    [SerializeField] private TMP_Text _firstText;
    [SerializeField] private TMP_Text _secondText;
    [SerializeField] private TMP_Text _thirdText;
    [SerializeField] private TMP_Text _fourthText;
    public void EndGame()
    {
        gameObject.SetActive(true);

        ShowPlayersScore();
    }

    private void ShowPlayersScore()
    {
        List<Player> players = _pointCounterScript.GetSortedPlayers();

        foreach (var player in players.Select((p, i) => new { Player = p, Position = i + 1 }))
        {
            switch (player.Player.Color)
            {
                case PlayerColor.BLUE:
                    switch (player.Position)
                    {
                        case 1:
                            _firstText.text = $"Winner: {player.Player.Name}";
                            _firstText.color = Color.blue;
                            break;
                        case 2:
                            _secondText.text = $"2nd: {player.Player.Name}";
                            _secondText.color = Color.blue;
                            break;
                        case 3:
                            _thirdText.text = $"3rd: {player.Player.Name}";
                            _thirdText.color = Color.blue;
                            break;
                        case 4:
                            _fourthText.text = $"4th: {player.Player.Name}";
                            _fourthText.color = Color.blue;
                            break;
                    }
                    break;
                case PlayerColor.RED:
                    switch (player.Position)
                    {
                        case 1:
                            _firstText.text = $"Winner: {player.Player.Name}";
                            _firstText.color = Color.red;
                            break;
                        case 2:
                            _secondText.text = $"2nd: {player.Player.Name}";
                            _secondText.color = Color.red;
                            break;
                        case 3:
                            _thirdText.text = $"3rd: {player.Player.Name}";
                            _thirdText.color = Color.red;
                            break;
                        case 4:
                            _fourthText.text = $"4th: {player.Player.Name}";
                            _fourthText.color = Color.red;
                            break;
                    }
                    break;
                case PlayerColor.GREEN:
                    switch (player.Position)
                    {
                        case 1:
                            _firstText.text = $"Winner: {player.Player.Name}";
                            _firstText.color = Color.green;
                            break;
                        case 2:
                            _secondText.text = $"2nd: {player.Player.Name}";
                            _secondText.color = Color.green;
                            break;
                        case 3:
                            _thirdText.text = $"3rd: {player.Player.Name}";
                            _thirdText.color = Color.green;
                            break;
                        case 4:
                            _fourthText.text = $"4th: {player.Player.Name}";
                            _fourthText.color = Color.green;
                            break;
                    }
                    break;
                case PlayerColor.YELLOW:
                    switch (player.Position)
                    {
                        case 1:
                            _firstText.text = $"Winner: {player.Player.Name}";
                            _firstText.color = Color.yellow;
                            break;
                        case 2:
                            _secondText.text = $"2nd: {player.Player.Name}";
                            _secondText.color = Color.yellow;
                            break;
                        case 3:
                            _thirdText.text = $"3rd: {player.Player.Name}";
                            _thirdText.color = Color.yellow;
                            break;
                        case 4:
                            _fourthText.text = $"4th: {player.Player.Name}";
                            _fourthText.color = Color.yellow;
                            break;
                    }
                    break;
            }
            Debug.Log($"Position {player.Position}: {player.Player.Name} with {player.Player.Points} points, {player.Player.TrunksTaken} trunkPoints, and {player.Player.WagonsStolen} wagons");
        }
    }
}
