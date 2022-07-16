using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGamebutton = null;
    private void Start() 
    {
        GameNetworkManager.ClientOnConnected += HandleClientConnected;
        
        GamePlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;

    }
    private void OnDestroy() 
    {
        GameNetworkManager.ClientOnConnected -= HandleClientConnected;

        GamePlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
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
        }
        else
        {
            NetworkManager.singleton.StopClient();

            lobbyUI.SetActive(false);
            landingPagePanel.SetActive(true);
        }
    }
}
