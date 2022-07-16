using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GamePlayer : NetworkBehaviour // 
{
    [SerializeField] private Transform cameraTransform = null; 
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private float buildingRangeLimit = 5f;
    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private NeutralBuilding[] nbuildings = new NeutralBuilding[0];
    [SerializeField] private List <Unit> myUnits = new List<Unit>();
    [SerializeField] private List<Building> myBuildings = new List<Building>();
    [SerializeField] private List<NeutralBuilding> neutralBuildings = new List<NeutralBuilding>();
    [SerializeField] private List<GameObject> neutralBuildingPoints = new List<GameObject>();

    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))] 
    private int resources = 10;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;

    public event Action<int> ClientOnResourcesUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

    public bool GetIsPartyOwner(){
        return isPartyOwner;
    }
    public Transform GetCameraTransform(){
        return cameraTransform;
    }
    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }
    public int GetResources()
    {
        return resources;
    }
    

    public bool CheckBuildable(BoxCollider buildingCollider, Vector3 position)
    {
        Collider[] colls = Physics.OverlapBox(
            position,
            buildingCollider.size / 1.1f,
            Quaternion.identity,
            buildingBlockLayer);
        

        if(colls.Length > 1)
        { 
            return false; 
        }


        foreach(Building building in myBuildings)
        {
            if(building.GetBuildingType() != Building.BuildingType.ZoneOccupier){ continue; }

            if((position -  building.transform.position).sqrMagnitude < (buildingRangeLimit * buildingRangeLimit))
            { 
                return true; 
            }
        }
        return false;
    }

    #region Server
    public override void OnStartServer()
    {
        GameNetworkManager.OnStartGame += ArrangeNeutralBuidling;
        Unit.ServerUnitSpawned += ServerHandleUnitSpawned; // subscribe 
        // Unit 에서는 gameplayer.cs 가 연결되어있는지 모름 
        // invoke 하면 event 가 발생하며 연결된 것들에게 전달
        // 함수 인자는 Action< > 에서 정해짐
        Unit.ServerUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerBuildingDespawned += ServerHandleBuildingDespawned;
        NeutralBuilding.ServerNeutralBuildingAdded += ServerHandleNeutralBuildingAdded;
        NeutralBuilding.ServerNeutralBuildingDespawned += ServerHandleNeutralBuildingDespawned;
       
        
    }

    public override void OnStopServer()
    {
        GameNetworkManager.OnStartGame -= ArrangeNeutralBuidling;
        Unit.ServerUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerBuildingDespawned -= ServerHandleBuildingDespawned;
        NeutralBuilding.ServerNeutralBuildingAdded -= ServerHandleNeutralBuildingAdded;
        NeutralBuilding.ServerNeutralBuildingDespawned -= ServerHandleNeutralBuildingDespawned;
    }

    [Server]
    public void SetResources(int value)
    {
        resources = value;
    }
    [Server]
    public void SetIsPartyOwner(bool state)
    {
        isPartyOwner = state;
    }

    private void ServerHandleUnitSpawned(Unit unit)
    {
        if(unit.connectionToClient.connectionId != connectionToClient.connectionId){ return; }

        myUnits.Add(unit);
    }
    private void ServerHandleUnitDespawned(Unit unit)
    {
        if(unit.connectionToClient.connectionId != connectionToClient.connectionId){ return; }

        myUnits.Remove(unit);
    }

    private void ServerHandleBuildingSpawned(Building building)
    {
        if(building.connectionToClient.connectionId != connectionToClient.connectionId){ return; }

        myBuildings.Add(building);
    }
    private void ServerHandleBuildingDespawned(Building building)
    {
        if(building.connectionToClient.connectionId != connectionToClient.connectionId){ return; }

        myBuildings.Remove(building);
    }

    private void ServerHandleNeutralBuildingAdded(NeutralBuilding building)
    {
        if(building.connectionToClient.connectionId != connectionToClient.connectionId){ return; }

        neutralBuildings.Add(building);
    }
    private void ServerHandleNeutralBuildingDespawned(NeutralBuilding building)
    {
        if(building.connectionToClient.connectionId != connectionToClient.connectionId){ return; }

        neutralBuildings.Remove(building);
    }

    [Command]
    public void CmdStartGame()
    {
        if(!isPartyOwner){ return; }

        ((GameNetworkManager)NetworkManager.singleton).StartGame();
    }
    [Command]
    public void CmdPlaceBuilding(int buildingId, Vector3 position)
    {
        Building buildingData = null;

        foreach(Building building in buildings)// 어떤 빌딩인지 id 로 찾기
        {
            if(building.GetId() == buildingId) 
            {
                buildingData = building;
                break;
            }
        }
        
        if(buildingData == null) { return; }

        if(resources < buildingData.GetPrice()) { return; }
        
        BoxCollider buildingCollider = buildingData.GetComponent<BoxCollider>();

        //Check whether the given box overlaps with other colliders or not.
        if(!CheckBuildable(buildingCollider, position)){ return; }

        GameObject buildingInstance = 
                    Instantiate(buildingData.gameObject, position, buildingData.transform.rotation);
        
        NetworkServer.Spawn(buildingInstance,connectionToClient);

        SetResources(resources - buildingData.GetPrice());

    }
    [Command]
    public void CmdSabotagePlaceBuilding(int buildingId, Vector3 position)
    {
        Building buildingData = null;

        foreach(Building building in buildings)// 어떤 빌딩인지 id 로 찾기
        {
            if(building.GetId() == buildingId) 
            {
                buildingData = building;
                break;
            }
        }
        if(buildingData == null) { return; }

        GameObject buildingInstance = 
                    Instantiate(buildingData.gameObject, position, buildingData.transform.rotation);
        
        NetworkServer.Spawn(buildingInstance,connectionToClient);
    }



    [Server]
    public void PlaceNeutralBuilding(int buildingId, Vector3 position)
    {
        NeutralBuilding buildingData = null;

        foreach(NeutralBuilding nbuilding in nbuildings)// 어떤 빌딩인지 id 로 찾기
        {
            if(nbuilding.GetId() == buildingId) 
            {
                buildingData = nbuilding;
                break;
            }
        }
        
        if(buildingData == null) { return; }

        GameObject buildingInstance = 
                    Instantiate(buildingData.gameObject, position, buildingData.transform.rotation);
        
        NetworkServer.Spawn(buildingInstance,connectionToClient);
    }

    [Server]
    private void ArrangeNeutralBuidling()
    {
        if(!hasAuthority){ return; }
        
        GameObject points = GameObject.Find("NeutralBuildingPoints");
        for(int i=0; i< points.transform.childCount; i++){
            neutralBuildingPoints.Add(points.transform.GetChild(i).gameObject);
        }

        foreach(GameObject neutralBuildingPoint in neutralBuildingPoints){
            PlaceNeutralBuilding(neutralBuildingPoint.GetComponent<NeutralBuildingPointId>().GetNeutralBuildingId(),
                             neutralBuildingPoint.transform.position);
        }
    }

    #endregion

    #region Client
    public override void OnStartAuthority() // will be called on the client that owns the object.
    {
        if(NetworkServer.active) { return; } // server x
        Unit.AuthorityUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityUnitDespawned += AuthorityHandleUnitDespawned;
        Building.AuthorityBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityBuildingDespawned += AuthorityHandleBuildingDespawned;
    }
    public override void OnStartClient()
    {
        if(NetworkServer.active) { return; } // server x

        ((GameNetworkManager)NetworkManager.singleton).Players.Add(this);
    }
    public override void OnStopClient()
    {
        if(!isClientOnly) { return; }

        ((GameNetworkManager)NetworkManager.singleton).Players.Remove(this);

        if(!hasAuthority) { return; }


        Unit.AuthorityUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.AuthorityBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    private void ClientHandleResourcesUpdated(int oldResources, int newResources)
    {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }
    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if(!hasAuthority) { return; }

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }
    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }
    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }
    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }
    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    #endregion
}
