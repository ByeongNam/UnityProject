using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Building : NetworkBehaviour
{
    public enum BuildingType
    {
        Spawner,
        Generator,
        ZoneOccupier,
        Upgrader,
        
    }
    [SerializeField] private GameObject buildingPreview = null;
    [SerializeField] private GameObject unitPreview = null;
    [SerializeField] private Sprite unitIcon = null;
    [SerializeField] private Sprite buildingIcon = null;
    [SerializeField] private Transform unitSpawnPoint = null;
    [SerializeField] private int id = -1;
    [SerializeField] private int price = 10;
    [SerializeField] private string unitId = "";
    [SerializeField] private BuildingType buildingType = new BuildingType();

    public static event Action<Building> ServerBuildingSpawned;
    public static event Action<Building> ServerBuildingDespawned;
    public static event Action<Building> AuthorityBuildingSpawned;
    public static event Action<Building> AuthorityBuildingDespawned;

    public BuildingType GetBuildingType(){
        return buildingType;
    } 
    public Sprite GetUnitIcon(){
        return unitIcon;
    }
    public Sprite GetBuildingIcon(){
        return buildingIcon;
    }
    public int GetPrice(){
        return price;
    }
    public int GetId(){
        return id;
    }
    public string GetUnitId(){
        return unitId;
    }

    public GameObject GetBuildingPreview(){
        return buildingPreview;
    }
    public GameObject GetUnitPreview(){
        return unitPreview;
    }
    public Transform GetUnitSpawnPoint(){
        return unitSpawnPoint;
    }

    #region Server

    public override void OnStartServer()
    {
        ServerBuildingSpawned?.Invoke(this);
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
              
        AuthorityBuildingSpawned?.Invoke(this);
    }
    public override void OnStopClient()
    {
        if(!hasAuthority){ return; }

        AuthorityBuildingDespawned?.Invoke(this);
    }

    #endregion

}
