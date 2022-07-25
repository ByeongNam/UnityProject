using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Upgrader : NetworkBehaviour
{
    private GamePlayer player;

    public override void OnStartServer()
    {
        player = connectionToClient.identity.GetComponent<GamePlayer>();
    }
    public override void OnStopServer()
    {

    }

    public void UpgradeResourceLimit()
    {
        
    }

    public void UpgradeUnit()
    {

    }

}
