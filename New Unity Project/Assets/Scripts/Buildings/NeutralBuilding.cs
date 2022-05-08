using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class NeutralBuilding : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Stat stat = null;
    [SerializeField] private UnityEvent OnBuildingSelected = null;
    //Unity 에서 제공하는 event
    [SerializeField] private UnityEvent OnBuildingDeselected = null;
    [SerializeField] private Sprite unitIcon = null;
    [SerializeField] private int id = -1;
    
    public static event Action<NeutralBuilding> ServerNeutralBuildingAdded;
    public static event Action<NeutralBuilding> ServerNeutralBuildingDespawned;
    
    public Sprite GetUnitIcon(){
        return unitIcon;
    }
    public int GetId(){
        return id;
    }
    #region Server
    public override void OnStartServer()
    {
        stat.CheckServerSabotageDie += HandleServerSabotageDie;
        ServerNeutralBuildingAdded?.Invoke(this);
    }
    public override void OnStopServer()
    {
        stat.CheckServerSabotageDie -= HandleServerSabotageDie;
        ServerNeutralBuildingDespawned?.Invoke(this);
    }
    private void Start()
    {
        OnBuildingDeselected?.Invoke();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnBuildingDeselected?.Invoke();
        }
    }


    [Server]
    private void HandleServerSabotageDie()
    {
        NetworkServer.Destroy(gameObject); 
    }

    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData) // UI 에서 마우스 클릭 이벤트를 감지
    {
        if (eventData.button != PointerEventData.InputButton.Left) { return; }

        OnBuildingSelected?.Invoke();

    }

    #endregion
}
