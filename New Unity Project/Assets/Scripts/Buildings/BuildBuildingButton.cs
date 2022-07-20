using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildBuildingButton : MonoBehaviour ,IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text priceText = null;
    [SerializeField] private TMP_Text nameText = null;
    [SerializeField] private LayerMask floorMask = new LayerMask();
    [SerializeField] private UnityEvent OnActivateNotEnoughResource = null;
    [SerializeField] private UnityEvent OnDisableNotEnoughResource = null;
    public static event Action ShowBuildableRange;
    public static event Action HideBuildableRange;

    private Camera mainCamera;
    private GamePlayer player;
    private GameObject buildingPreviewInstance;
    private Renderer buildingRendererInstance;
    private BoxCollider buildingCollider;

    

    private void Start() 
    {
        mainCamera =  Camera.main;
        
        iconImage.sprite = building.GetUnitIcon();
        priceText.text = building.GetPrice().ToString();
        nameText.text = building.GetBuildingName();
        player = NetworkClient.connection.identity.GetComponent<GamePlayer>();
        buildingCollider = building.GetComponent<BoxCollider>();
    }

    private void Update() 
    {
        if(buildingPreviewInstance == null) { return; }

        UpdateBuildingPreview();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) { return; }

        if(player.GetResources() < building.GetPrice()){
            OnActivateNotEnoughResource?.Invoke();
            StartCoroutine(DisableNotEnoughResource());
            return; 
        }
        ShowBuildableRange?.Invoke();
        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<MeshRenderer>();
        buildingPreviewInstance.GetComponent<BoxCollider>().isTrigger = true;
        buildingPreviewInstance.GetComponent<NavMeshObstacle>().enabled = false;
        buildingPreviewInstance.SetActive(false);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if(buildingPreviewInstance == null) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            player.CmdPlaceBuilding(building.GetId(), hit.point);
        }

        HideBuildableRange?.Invoke();
        Destroy(buildingPreviewInstance);
    }

    IEnumerator DisableNotEnoughResource(){
        yield return new WaitForSeconds(1);
        OnDisableNotEnoughResource?.Invoke();
    }

    private void UpdateBuildingPreview(){
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)){ return; }

        buildingPreviewInstance.transform.position = hit.point;

        if(!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }

        Color color = player.CheckBuildable(buildingCollider, hit.point) ? Color.green : Color.red;

        buildingRendererInstance.material.SetColor("_BaseColor",color);
    }
}
