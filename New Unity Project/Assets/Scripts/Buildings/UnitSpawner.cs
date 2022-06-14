using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour
{
    [SerializeField] private Building building = null;
    [SerializeField] private Stat stat = null;
    [SerializeField] private GameObject unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;
    [SerializeField] private TMP_Text unitQueueCountText = null;
    [SerializeField] private Image unitProgressImage = null;
    [SerializeField] private Image buildingIcon = null;
    [SerializeField] private TMP_Text buildingName = null;
    [SerializeField] private Image unitIcon = null;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 1f;
    [SerializeField] private float unitSpawnDelay = 10f;

    [SerializeField] private UnityEvent OnBuildingSelected = null;
    //Unity 에서 제공하는 event
    [SerializeField] private UnityEvent OnBuildingDeselected = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    [SerializeField] private bool isSpawnable = true;
    
    [SyncVar(hook = nameof(ClientHandleQueuedUnitUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    private Camera mainCamera;
    private float progressImageVelocity;

    public bool GetIsSpawnable()
    {
        return isSpawnable;
    }
    private void Start()
    {
        mainCamera = Camera.main;
        OnBuildingDeselected?.Invoke();
        buildingIcon.sprite = building.GetBuildingIcon();
        buildingName.text = building.GetId().ToString();// 빌딩이름으로 변경
        if(unitIcon != null)
        {
            unitIcon.sprite = building.GetUnitIcon();
        }
        
    }
    private void Update()
    {
        if(isServer && isSpawnable)
        {
            ProduceUnit();
        }
        if(isClient && isSpawnable)
        {
            UpdateTimerDisplay();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {   
            if(!hasAuthority){ return; }
            
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
    private void ProduceUnit()
    {
        if(queuedUnits == 0){ return; }

        unitTimer += Time.deltaTime;

        if(unitTimer < unitSpawnDelay) { return; }

        GameObject unitInstance = Instantiate(
            unitPrefab,
            unitSpawnPoint.position,
            unitSpawnPoint.rotation);

        NetworkServer.Spawn(unitInstance, connectionToClient);
        // Spawn the given game object on all clients which are ready
        // connectionToClient는 서버 측에 있는 플레이어 객체에서만 유효하다.
        Vector3 randomSpawn = Random.insideUnitSphere * spawnMoveRange;
        randomSpawn.y = unitSpawnPoint.position.y;
        
        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + randomSpawn);

        queuedUnits--;
        unitTimer = 0f;
    }

    [Server]
    private void HandleServerDie()
    {
        NetworkServer.Destroy(gameObject); // building destroy handle 건물 파괴시
    }
    

    [Command]
    public void CmdSpawnUnit()
    {
        if(queuedUnits == maxUnitQueue){ return; }

        GamePlayer gamePlayer = connectionToClient.identity.GetComponent<GamePlayer>();

        if(gamePlayer.GetResources() < unitPrefab.GetComponent<Unit>().GetResourceCost()){ return; }

        queuedUnits++;

        gamePlayer.SetResources(gamePlayer.GetResources() - unitPrefab.GetComponent<Unit>().GetResourceCost());
    }

    #endregion

    #region Client
    private void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDelay;

        if(newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(
                unitProgressImage.fillAmount,
                newProgress,
                ref progressImageVelocity,
                0.1f
            );
        }

    }
    private void ClientHandleQueuedUnitUpdated(int oldValue, int newValue)
    {
        unitQueueCountText.text = newValue.ToString();
    }
    #endregion
}
