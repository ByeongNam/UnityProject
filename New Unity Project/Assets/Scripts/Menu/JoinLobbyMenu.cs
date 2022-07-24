using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private TMP_InputField addressInput = null;
    [SerializeField] private Button joinButton = null;
    [SerializeField] private Button exitButton = null;
    [SerializeField] private LobbyCameraController lobbyCameraController = null;

    private void OnEnable() 
    {
        GameNetworkManager.ClientOnConnected += HandleClientConnected;
        GameNetworkManager.ClientOnDisconnected += HandleClientDisconnected;
    }

    private void OnDisable() 
    {
        GameNetworkManager.ClientOnConnected -= HandleClientConnected;
        GameNetworkManager.ClientOnDisconnected -= HandleClientDisconnected;
    }

    public void Join()
    {
        string address = addressInput.text;

        NetworkManager.singleton.networkAddress = address;
        NetworkManager.singleton.StartClient();
        
         
        joinButton.interactable = false;
        exitButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        lobbyCameraController.MoveToPosition(3);
        joinButton.interactable = true;
        exitButton.interactable = true;

        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);

    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
        exitButton.interactable = true;
    }
}
