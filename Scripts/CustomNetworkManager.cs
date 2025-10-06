using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField] private PlayerObjectController GamePlayerPrefab;
    public List<PlayerObjectController> GamePlayers { get; } = new List<PlayerObjectController>();
    private GameObject _gameCanvas;
    private GameObject _lobbyCanvas;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            Debug.Log("On server add player");
            if (_gameCanvas == null)
            {
                _gameCanvas = GameObject.Find("Game Canvas");
                _gameCanvas?.SetActive(false);
            }

            Debug.Log("GamePlayer.Count = " + GamePlayers.Count);
            PlayerObjectController gamePlayerInstance = Instantiate(GamePlayerPrefab);
            gamePlayerInstance.ConnectionID = conn.connectionId;
            gamePlayerInstance.PlayerIdNumber = GamePlayers.Count + 1;
            gamePlayerInstance.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.Instance.CurrentLobbyID, GamePlayers.Count);
            gamePlayerInstance.PlayerColor = (PlayerColor)GamePlayers.Count;
            Debug.Log("Player Color: " + gamePlayerInstance.PlayerColor);

            NetworkServer.AddPlayerForConnection(conn, gamePlayerInstance.gameObject);

        }
    }

    public void StartGame(string sceneName)
    {
        ServerChangeScene(sceneName);
    }

    public void StartGame()
    {
        Debug.Log("Start game custom network manager");
        _lobbyCanvas = GameObject.Find("Lobby Canvas");
        _lobbyCanvas.SetActive(false);

        _gameCanvas.SetActive(true);
        _gameCanvas.GetComponent<GameManager>().StartGame(GamePlayers.Count);
    }
}
