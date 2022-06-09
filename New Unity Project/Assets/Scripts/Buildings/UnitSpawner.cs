using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UnitSpawner : NetworkBehaviour
{
    [SerializeField] private Building building = null;
    [SerializeField] private Stat stat = null;
    [SerializeField] private GameObject unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;

    [SerializeField] private UnityEvent OnBuildingSelected = null;
    //Unity 에서 제공하는 event
    [SerializeField] private UnityEvent OnBuildingDeselected = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    private Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;
        OnBuildingDeselected?.Invoke();
    }
    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {   
            
            if(!IsPointerOverUIObject()){ OnBuildingDeselected?.Invoke(); }
            
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)){
                //Debug.Log($"hit: {hit.transform.gameObject.GetInstanceID()} , this: {transform.gameObject.GetInstanceID()}");
                if(hit.transform.gameObject.GetInstanceID() == transform.gameObject.GetInstanceID()){
                    OnBuildingDeselected?.Invoke();
                    //Debug.Log("clicked: " + building.GetId() + ", owner: "+ connectionToClient.connectionId);
                    OnBuildingSelected?.Invoke(); 
                }
            }
        }
    }
    private bool IsPointerOverUIObject()
    {
        bool flag = false;
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue());
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        foreach(RaycastResult result in results){
            if(result.gameObject.CompareTag("UI")){
                flag = true;
            }
            else if(result.gameObject.CompareTag("Building")){
                flag = false;
            }
        }
        return flag;
    }

    #region Server
    public override void OnStartServer()
    {
        stat.CheckServerDie += HandleServerDie;
    }
    public override void OnStopServer()
    {
        stat.CheckServerDie -= HandleServerDie;
    }


    [Server]
    private void HandleServerDie()
    {
        NetworkServer.Destroy(gameObject); // building destroy handle 건물 파괴시
    }
    

    [Command]
    public void CmdSpawnUnit()
    {
        GameObject unitInstance = Instantiate(
            unitPrefab,
            unitSpawnPoint.position,
            unitSpawnPoint.rotation);

        NetworkServer.Spawn(unitInstance, connectionToClient);
        // Spawn the given game object on all clients which are ready
        // connectionToClient는 서버 측에 있는 플레이어 객체에서만 유효하다.

    }

    #endregion

    #region Client

    #endregion
}
