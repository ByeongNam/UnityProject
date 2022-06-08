using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitBase : NetworkBehaviour
{
    [SerializeField] private Stat stat = null;
    [SerializeField] private Targetable targetable = null;
    public static event Action<UnitBase> ServerBaseSpawned;
    public static event Action<UnitBase> ServerBaseDespawned;
    public static event Action<int> ServerPlayerDie;


    #region Server
    public override void OnStartServer()
    {
        stat.CheckServerDie += HandleServerDie;

        ServerBaseSpawned?.Invoke(this);
    }
    public override void OnStopServer()
    {
        stat.CheckServerDie -= HandleServerDie;

        ServerBaseDespawned?.Invoke(this);
    }

    [Server]
    private void HandleServerDie()
    {
        ServerPlayerDie?.Invoke(connectionToClient.connectionId);
        NetworkServer.Destroy(gameObject);
    }
   

    #endregion
    #region Client

    #endregion
}
