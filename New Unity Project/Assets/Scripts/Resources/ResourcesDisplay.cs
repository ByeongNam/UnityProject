using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText = null;

    private GamePlayer player;
    private void Start() 
    {
        player = NetworkClient.connection.identity.GetComponent<GamePlayer>();
        ClientHandleResourcesUpdated(player.GetResources());
        player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
    }
    private void OnDestroy() {
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
    }

    private void ClientHandleResourcesUpdated(int resources)
    {
        resourcesText.text = resources.ToString();
    }
}
