using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private LobbyCameraController lobbyCameraController = null;
    [SerializeField] private GameObject startPagePanel = null;
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGamebutton = null;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[2];
    private void Start() 
    {
        GameNetworkManager.ClientOnConnected += HandleClientConnected;
        
        GamePlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;

        GamePlayer.ClientOnInfoUpdated += CliendtHandleInfoUpdated;

    }
    private void OnDestroy() 
    {
        GameNetworkManager.ClientOnConnected -= HandleClientConnected;

        GamePlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;

        GamePlayer.ClientOnInfoUpdated -= CliendtHandleInfoUpdated;
    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    private void CliendtHandleInfoUpdated()
    {
        List<GamePlayer> players = ((GameNetworkManager)NetworkManager.singleton).Players;

        if(players.Count <= 1)
        {
            playerNameTexts[0].text = players[0].GetDisplayName();
            playerNameTexts[1].text = "None";
        }
        else
        {
            for(int i=0 ; i< players.Count; i++)
            {
                playerNameTexts[i].text = players[i].GetDisplayName();
            }            
        }

        startGamebutton.interactable = players.Count == 2;
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        startGamebutton.gameObject.SetActive(state);
    }

    public void StartGame()
    {
        NetworkClient.connection.identity.GetComponent<GamePlayer>().CmdStartGame();
    }
    public void LeaveLobby()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
            startPagePanel.SetActive(false);
            landingPagePanel.SetActive(true);
            lobbyCameraController.MoveToPosition(1);
        }
        else
        {
            NetworkManager.singleton.StopClient();
            lobbyCameraController.MoveToPosition(1);
            lobbyUI.SetActive(false);
            landingPagePanel.SetActive(true);
        }
    }
}
