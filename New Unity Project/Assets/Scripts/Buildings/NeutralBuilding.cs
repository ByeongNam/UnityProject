using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NeutralBuilding : NetworkBehaviour
{
    [SerializeField] private String buildingName = null;
    [SerializeField] private Stat stat = null;
    [SerializeField] private UnityEvent OnBuildingSelected = null;
    //Unity 에서 제공하는 event
    [SerializeField] private UnityEvent OnBuildingDeselected = null;
    [SerializeField] private Sprite unitIcon = null;
    [SerializeField] private Sprite buildingIcon = null;
    [SerializeField] private Image buildingIconImage = null;
    [SerializeField] private TMP_Text buildingNameText = null;
    [SerializeField] private int id = -1;
    
    public static event Action<NeutralBuilding> ServerNeutralBuildingAdded;
    public static event Action<NeutralBuilding> ServerNeutralBuildingDespawned;

    [SerializeField] private LayerMask layerMask = new LayerMask();
    private Camera mainCamera;
    
    public Sprite GetUnitIcon(){
        return unitIcon;
    }
    public int GetId(){
        return id;
    }
    #region Server
    public override void OnStartServer()
    {
        stat.CheckServerDie += HandleServerDie;
        stat.CheckServerSabotageDie += HandleServerSabotageDie;
        ServerNeutralBuildingAdded?.Invoke(this);
    }
    public override void OnStopServer()
    {
        stat.CheckServerDie -= HandleServerDie;
        stat.CheckServerSabotageDie -= HandleServerSabotageDie;
        ServerNeutralBuildingDespawned?.Invoke(this);
    }
    private void Start()
    {
        mainCamera = Camera.main;
        OnBuildingDeselected?.Invoke();
        buildingIconImage.sprite = buildingIcon;
        buildingNameText.text = buildingName;
    }
    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {   
            if(!IsPointerOverUIObject()){ OnBuildingDeselected?.Invoke(); }
            
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)){ 
                if(hit.transform.gameObject == transform.gameObject){
                    OnBuildingDeselected?.Invoke();
                    Debug.Log($"clicked: {id}, owner: {connectionToClient.connectionId}");
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

    [Server]
    private void HandleServerDie()
    {
        NetworkServer.Destroy(gameObject);
    }


    [Server]
    private void HandleServerSabotageDie()
    {
        NetworkServer.Destroy(gameObject); 
    }

    #endregion

    #region Client

    #endregion
}
