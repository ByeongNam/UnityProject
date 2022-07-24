using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText = null;
    [SerializeField] private TMP_Text resourceLimitText = null;

    private GamePlayer player;
    private void Start() 
    {
        player = NetworkClient.connection.identity.GetComponent<GamePlayer>();
        ClientHandleResourcesUpdated(player.GetResources());
        HandleResourceLimit(player.GetResourceLimit());
        player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
        player.ClientOnResourceLimitUpdated += HandleResourceLimit;
    }
    private void OnDestroy() {
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
        player.ClientOnResourceLimitUpdated -= HandleResourceLimit;
    }

    private void ClientHandleResourcesUpdated(int resources)
    {
        resourcesText.text = resources.ToString();
        if(resources == player.GetResourceLimit()){
            resourcesText.color = Color.red;
        }
        else{
            resourcesText.color = Color.black;
        }
    }
    private void HandleResourceLimit(int limit)
    {
        resourceLimitText.text = limit.ToString();
    }
}
