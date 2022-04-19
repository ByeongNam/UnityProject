using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform unitSelectionArea = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    // layermask

    private Vector2 startPosition;
    private GamePlayer player = null;
    private Camera mainCamera;
    [SerializeField] public List <Unit> SelectedUnits = new List<Unit>(); // data 을 읽을 수는 있는 접근자 get
    // 커서로 선택된 유닛들 배열(다른 cs 에서 쓰임)
    private void Awake() {
        Unit.SelectedUnitDespawned += RemoveUnitFromSelectedUnits;
    }
    private void Start()
    {
        mainCamera = Camera.main;
        player = NetworkClient.connection.identity.GetComponent<GamePlayer>();
        GameOverHandler.ClientGameOver += ClientHandleGameOver;
    }
    private void OnDestroy() {
        Unit.SelectedUnitDespawned -= RemoveUnitFromSelectedUnits;
        GameOverHandler.ClientGameOver -= ClientHandleGameOver;
    }
    private void Update() 
    {
        if(player == null){
            player = NetworkClient.connection.identity.GetComponent<GamePlayer>();
        }
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if(Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if(Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }    
    }
    private void StartSelectionArea()
    {
        if(!Keyboard.current.leftShiftKey.isPressed) // shift 키로 연속 선택
        {
             foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Deselect();
            }

            SelectedUnits.Clear();   
        }
        unitSelectionArea.gameObject.SetActive(true);

        startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }
    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth),Mathf.Abs(areaHeight));

        unitSelectionArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);

    }
    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);
        if(unitSelectionArea.sizeDelta.magnitude == 0)// just click
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)){ return; } 
            // collider 충돌
            if(!hit.collider.TryGetComponent<Unit>(out Unit unit)){ return; }

            if(!unit.hasAuthority){ return; }

            SelectedUnits.Add(unit);

            foreach(Unit selectedUnit in SelectedUnits) // 선택된 유닛들에 대해 select 함수 호출
            {
                selectedUnit.Select();
            }

            return ;
        }
        
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);
        
        foreach(Unit unit in player.GetMyUnits())
        {
            if(SelectedUnits.Contains(unit)){ continue; } // 리스트에 unit 이 이미 존재할 경우 
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            if(screenPosition.x > min.x &&
               screenPosition.x < max.x && 
               screenPosition.y > min.y &&
               screenPosition.y < max.y)// 드래그 범위 내
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }
    public void RemoveUnitFromSelectedUnits(Unit unit){
        SelectedUnits.Remove(unit); 
    }

    private void ClientHandleGameOver(string winner){
        //enabled = false;
    }
}
