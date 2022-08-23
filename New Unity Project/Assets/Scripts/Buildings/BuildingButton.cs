using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building = null;
    [SerializeField] private UnitSpawner unitSpawner = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text functionTextUnit = null;
    //[SerializeField] private LayerMask floorMask = new LayerMask();
    

    private GamePlayer player;

    private GameObject unitPreviewObject;
    private Renderer unitPreviewObjectRenderer;

    private void Start() 
    {
        iconImage.sprite = building.GetUnitIcon();

        player = NetworkClient.connection.identity.GetComponent<GamePlayer>();

        if(functionTextUnit == null) { return; }

            // unit name handling
        functionTextUnit.text = building.GetUnitId();
    }
    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.F)){
            OnFKeyDown();
        }
        if(Input.GetKeyUp(KeyCode.F)){
            OnFKeyUp();
        }
        if(unitPreviewObject == null){ return; }
    }
    
    public void OnFKeyDown()
    {
        if(unitPreviewObject != null){ return; }
        if(!unitSpawner.GetIsSpawnable()){ return; }
        unitPreviewObject = Instantiate(building.GetUnitPreview());
        unitPreviewObjectRenderer = unitPreviewObject.GetComponentInChildren<Renderer>();
        unitPreviewObject.transform.position = building.GetUnitSpawnPoint().position;
        unitPreviewObject.SetActive(true);
    }

    public void OnFKeyUp()
    {
        if(unitPreviewObject == null){ return; }
        if(!unitSpawner.GetIsSpawnable()){ return; }
        Destroy(unitPreviewObject);
        unitSpawner.CmdSpawnUnit();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(unitPreviewObject != null){ return; }
        if(!unitSpawner.GetIsSpawnable()){ return; }
        unitPreviewObject = Instantiate(building.GetUnitPreview());
        unitPreviewObjectRenderer = unitPreviewObject.GetComponentInChildren<Renderer>();
        unitPreviewObject.transform.position = building.GetUnitSpawnPoint().position;
        unitPreviewObject.SetActive(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(unitPreviewObject == null){ return; }
        if(!unitSpawner.GetIsSpawnable()){ return; }
        Destroy(unitPreviewObject);
        unitSpawner.CmdSpawnUnit();
    }
}
