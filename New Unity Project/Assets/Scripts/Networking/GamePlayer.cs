using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GamePlayer : NetworkBehaviour // 
{
    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private NeutralBuilding[] nbuildings = new NeutralBuilding[0];
    [SerializeField] private List <Unit> myUnits = new List<Unit>();
    [SerializeField] private List<Building> myBuildings = new List<Building>();
    [SerializeField] private List<NeutralBuilding> neutralBuildings = new List<NeutralBuilding>();
    [SerializeField] private List<GameObject> neutralBuildingPoints = new List<GameObject>();
    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    #region Server
    public override void OnStartServer()
    {
        Unit.ServerUnitSpawned += ServerHandleUnitSpawned; // subscribe 
        // Unit 에서는 gameplayer.cs 가 연결되어있는지 모름 
        // invoke 하면 event 가 발생하며 연결된 것들에게 전달
        // 함수 인자는 Action< > 에서 정해짐
        Unit.ServerUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerBuildingAdded += ServerHandleBuildingAdded;
        Building.ServerBuildingDespawned += ServerHandleBuildingDespawned;
        NeutralBuilding.ServerNeutralBuildingAdded += ServerHandleNeutralBuildingAdded;
        NeutralBuilding.ServerNeutralBuildingDespawned += ServerHandleNeutralBuildingDespawned;
        if(!hasAuthority){ return; }
        GameObject points = GameObject.Find("NeutralBuildingPoints");
        for(int i=0; i< points.transform.childCount; i++){
            neutralBuildingPoints.Add(points.transform.GetChild(i).gameObject);
        }

        foreach(GameObject neutralBuildingPoint in neutralBuildingPoints){
            CmdPlaceNeutralBuilding(neutralBuildingPoint.GetComponent<NeutralBuildingPointId>().GetNeutralBuildingId(),
                             neutralBuildingPoint.transform.position);
        }
    }

    public override void OnStopServer()
    {
        Unit.ServerUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerBuildingAdded -= ServerHandleBuildingAdded;
        Building.ServerBuildingDespawned -= ServerHandleBuildingDespawned;
        NeutralBuilding.ServerNeutralBuildingAdded -= ServerHandleNeutralBuildingAdded;
        NeutralBuilding.ServerNeutralBuildingDespawned -= ServerHandleNeutralBuildingDespawned;
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

    private void ServerHandleBuildingAdded(Building building)
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

        GameObject buildingInstance = 
                    Instantiate(buildingData.gameObject, position, buildingData.transform.rotation);
        
        NetworkServer.Spawn(buildingInstance,connectionToClient);
    }
    [Server]
    public void CmdPlaceNeutralBuilding(int buildingId, Vector3 position)
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

    #endregion

    #region Client
    public override void OnStartAuthority() // will be called on the client that owns the object.
    {
        if(NetworkServer.active) { return; } // server x
        Unit.AuthorityUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityUnitDespawned += AuthorityHandleUnitDespawned;
        Building.AuthorityBuildingAdded += AuthorityHandleBuildingAdded;
        Building.AuthorityBuildingDespawned += AuthorityHandleBuildingDespawned;
    }
    public override void OnStopClient()
    {
        if(!isClientOnly || !hasAuthority) { return; }
        Unit.AuthorityUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.AuthorityBuildingAdded -= AuthorityHandleBuildingAdded;
        Building.AuthorityBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }
    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }
    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }
    private void AuthorityHandleBuildingAdded(Building building)
    {
        myBuildings.Add(building);
    }
    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    #endregion
}
