using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Building : NetworkBehaviour
{
    [SerializeField] private GameObject unitPreview = null;
    [SerializeField] private GameObject sabotageArea = null;
    [SerializeField] private Sprite unitIcon = null;
    [SerializeField] private Transform unitSpawnPoint = null;
    [SerializeField] private int id = -1;
    [SerializeField] private string unitId = "";

    public static event Action<Building> ServerBuildingAdded;
    public static event Action<Building> ServerBuildingDespawned;
    public static event Action<Building> AuthorityBuildingAdded;
    public static event Action<Building> AuthorityBuildingDespawned;

    public Sprite GetUnitIcon(){
        return unitIcon;
    }
    public int GetId(){
        return id;
    }
    public string GetUnitId(){
        return unitId;
    }

    public GameObject GetUnitPreview(){
        return unitPreview;
    }
    public GameObject GetSabotageArea(){
        return sabotageArea;
    }
    public Transform GetUnitSpawnPoint(){
        return unitSpawnPoint;
    }

    #region Server

    public override void OnStartServer()
    {
        ServerBuildingAdded?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerBuildingDespawned?.Invoke(this);
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        if(!hasAuthority){ return; } // server x
              
        AuthorityBuildingAdded?.Invoke(this);
    }
    public override void OnStopClient()
    {
        if(!hasAuthority){ return; }

        AuthorityBuildingDespawned?.Invoke(this);
    }

    #endregion

}
