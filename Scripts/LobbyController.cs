using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance;

    //UI Elements
    public TMP_Text LobbyNameText;

    //Player Data
    public GameObject PlayerListViewContent;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;

    //Other Data
    public ulong CurrentLobbyID;
    public bool PlayerItemCreated = false;
    private List<PlayerListItem> PlayerListItems = new List<PlayerListItem>();
    public PlayerObjectController LocalPlayerController;

    //Ready
    public Button StartGameButton;
    public TMP_Text ReadyButtonText; 

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
    }

    public void ReadyPlayer()
    {
        Debug.Log("Readied");
        LocalPlayerController.ChangeReady();
    }

    public void UpdateButton()
    {
        if (LocalPlayerController.Ready)
        {
            ReadyButtonText.text = "Not ready";
        }
        else
        {
            ReadyButtonText.text = "Ready";
        }
    }

    public void CheckIfAllReady()
    {
        bool allReady = false;
        foreach (var player in Manager.GamePlayers)
        {
            if (player.Ready)
            {
                allReady = true;
            }
            else
            {
                allReady = false;
                break;
            }
        }
        
        if (allReady)
        {
            if (LocalPlayerController.PlayerIdNumber == 1)
            {
                StartGameButton.interactable = true;
            }
            else
            {
                StartGameButton.interactable = false;
            }
        }
        else
        {
            StartGameButton.interactable = false;
        }
    }

    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
    }

    public void UpdatePlayerList()
    {
        Debug.Log("UpdatePlayerList");
        Debug.Log("PlayerListItems.Count: " + PlayerListItems.Count + ", Manager.GamePlayers.Count: " + Manager.GamePlayers.Count);

        if (!PlayerItemCreated) { CreateHostPlayerItem(); }
        if (PlayerListItems.Count < Manager.GamePlayers.Count) { CreateClientPlayerItem(); }
        if (PlayerListItems.Count > Manager.GamePlayers.Count) { RemovePlayerItem(); }
        if (PlayerListItems.Count == Manager.GamePlayers.Count) { UpdatePlayerItem(); }
    }

    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalPlayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

    public void CreateHostPlayerItem()
    {
        Debug.Log("CreateHostPlayerItem");
        foreach (var player in Manager.GamePlayers)
        {
            GameObject newPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
            PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();

            newPlayerItemScript.PlayerName = player.PlayerName;
            newPlayerItemScript.ConnectionID = player.ConnectionID;
            newPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
            newPlayerItemScript.Ready = player.Ready;
            newPlayerItemScript.SetPlayerValues();

            newPlayerItem.transform.SetParent(PlayerListViewContent.transform);
            newPlayerItem.transform.localScale = Vector3.one;

            PlayerListItems.Add(newPlayerItemScript);
        }
        PlayerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        Debug.Log("CreateClientPlayerItem");
        foreach (var player in Manager.GamePlayers)
        {
            if (!PlayerListItems.Any(b => b.ConnectionID == player.ConnectionID))
            {
                GameObject newPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
                PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();

                newPlayerItemScript.PlayerName = player.PlayerName;
                newPlayerItemScript.ConnectionID = player.ConnectionID;
                newPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
                newPlayerItemScript.Ready = player.Ready;
                newPlayerItemScript.SetPlayerValues();

                newPlayerItem.transform.SetParent(PlayerListViewContent.transform);
                newPlayerItem.transform.localScale = Vector3.one;

                PlayerListItems.Add(newPlayerItemScript);
                Debug.Log("hehehehehe");
            }
        }
        
    }

    public void UpdatePlayerItem()
    {
        Debug.Log("UpdatePlayerItem");
        foreach (var player in Manager.GamePlayers)
        {
            foreach (var playerListItemScript in PlayerListItems)
            {
                if (playerListItemScript.ConnectionID == player.ConnectionID)
                {
                    playerListItemScript.PlayerName = player.PlayerName;
                    playerListItemScript.Ready = player.Ready;
                    playerListItemScript.SetPlayerValues();
                    if (player == LocalPlayerController)
                    {
                        UpdateButton();
                    }
                }
            }
        }

        CheckIfAllReady();
    }

    public void RemovePlayerItem()
    {
        Debug.Log("RemovePlayerItem");
        List<PlayerListItem> playerListItemsToRemove = new List<PlayerListItem>();

        foreach (var playerListItem in PlayerListItems)
        {
            if (!Manager.GamePlayers.Any(b => b.ConnectionID == playerListItem.ConnectionID))
            {
                playerListItemsToRemove.Add(playerListItem);
            }
        }
        if (playerListItemsToRemove.Count > 0)
        {
            foreach (var itemToRemove in playerListItemsToRemove)
            {
                GameObject objectToRemove = itemToRemove.gameObject;
                PlayerListItems.Remove(itemToRemove);
                //Destroy(objectToRemove);
                objectToRemove = null;
            }
        }
    }

    public void StartGame(string sceneName)
    {
        ForwardPlayerIconsToManager();
        LocalPlayerController.CanStartGame(sceneName);
    }

    public RawImage GetLastPlayerIcon()
    {
        return PlayerListItems.Last().PlayerIcon;
    }

    private void ForwardPlayerIconsToManager()
    {
        for (int i = 0; i < PlayerListItems.Count; i++)
        {
            Manager.GamePlayers[i].PlayerIcon = PlayerListItems[i].PlayerIcon;
        }
    }
}
