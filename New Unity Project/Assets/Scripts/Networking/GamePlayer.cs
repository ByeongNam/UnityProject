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
    [SyncVar(hook = nameof(ClientHandleResourceLimitUpdated))]
    private int resourceLimit = 30;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;
    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string displayName;

    private int realResourceLimit = 70;
    

    public event Action<int> ClientOnResourcesUpdated;
    public event Action<int> ClientOnResourceLimitUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;
    public static event Action ClientOnInfoUpdated;

    public int GetResourceLimit(){
        return resourceLimit;
    }
    public string GetDisplayName(){
        return displayName;
    }
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
        
        Unit.ServerUnitSpawned += ServerHandleUnitSpawned; // subscribe 
        // Unit ????????? gameplayer.cs ??? ????????????????????? ?????? 
        // invoke ?????? event ??? ???????????? ????????? ???????????? ??????
        // ?????? ????????? Action< > ?????? ?????????
        Unit.ServerUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerBuildingDespawned += ServerHandleBuildingDespawned;
        NeutralBuilding.ServerNeutralBuildingAdded += ServerHandleNeutralBuildingAdded;
        NeutralBuilding.ServerNeutralBuildingDespawned += ServerHandleNeutralBuildingDespawned;

        GameStartMenu.OnGameStartSetting += ArrangeNeutralBuidling;

        DontDestroyOnLoad(gameObject); // ?????? ????????? ???????????? Destroy ?????? ??????
    }

    public override void OnStopServer()
    {
        Unit.ServerUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerBuildingDespawned -= ServerHandleBuildingDespawned;
        NeutralBuilding.ServerNeutralBuildingAdded -= ServerHandleNeutralBuildingAdded;
        NeutralBuilding.ServerNeutralBuildingDespawned -= ServerHandleNeutralBuildingDespawned;

        GameStartMenu.OnGameStartSetting -= ArrangeNeutralBuidling;

    }

    

    [Server]
    public void SetDisplayName(string name)
    {
        displayName = name;
    }
    [Server]
    public void SetResources(int value)
    {
        if(resourceLimit < value) { return; }
        resources = value;
    }
    [Server]
    public void SetResourceLimit(int value)
    {
        if(realResourceLimit < value) { return; }
        resourceLimit = value;
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

        foreach(Building building in buildings)// ?????? ???????????? id ??? ??????
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

        foreach(Building building in buildings)// ?????? ???????????? id ??? ??????
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

        foreach(NeutralBuilding nbuilding in nbuildings)// ?????? ???????????? id ??? ??????
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

        DontDestroyOnLoad(gameObject);

        ((GameNetworkManager)NetworkManager.singleton).Players.Add(this);
    }
    public override void OnStopClient()
    {
        ClientOnInfoUpdated?.Invoke();

        if(!isClientOnly) { return; }

        ((GameNetworkManager)NetworkManager.singleton).Players.Remove(this);

        if(!hasAuthority) { return; }


        Unit.AuthorityUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.AuthorityBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }



    private void ClientHandleResourcesUpdated(int oldResources, int newResources) // hook
    {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }
    private void ClientHandleResourceLimitUpdated(int oldLimit, int newLimit) // hook
    {
        ClientOnResourceLimitUpdated?.Invoke(newLimit);
    }
    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState) // hook
    {
        if(!hasAuthority) { return; }

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }
    private void ClientHandleDisplayNameUpdated(string oldName, string newName)
    {
        ClientOnInfoUpdated?.Invoke();
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
