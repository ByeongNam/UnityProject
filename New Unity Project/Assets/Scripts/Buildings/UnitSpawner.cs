using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Stat stat = null;
    [SerializeField] private GameObject unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;

    [SerializeField] private UnityEvent OnBuildingSelected = null;
    //Unity 에서 제공하는 event
    [SerializeField] private UnityEvent OnBuildingDeselected = null;

    private void Start()
    {
        OnBuildingDeselected?.Invoke();
    }
    private void Update()
    {
        if (!hasAuthority) { return; }
        if (Input.GetMouseButtonDown(0))
        {
            OnBuildingDeselected?.Invoke();
        }
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

    public void OnPointerClick(PointerEventData eventData) // UI 에서 마우스 클릭 이벤트를 감지
    {
        if (eventData.button != PointerEventData.InputButton.Left) { return; }

        if (!hasAuthority) { return; }

        OnBuildingSelected?.Invoke();

    }
    #endregion
}
