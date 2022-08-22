using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    [SerializeField] private GameObject moveObject = null;
    [SerializeField] private GameObject autoAttackMode = null;

    private bool autoAttackingMode = false;
    private bool attackNearest = false;
    private Camera mainCamera;
    private void Start() {
        mainCamera = Camera.main;
        GameOverHandler.ClientGameOver += ClientHandleGameOver;
    }

    private void OnDestroy(){
        GameOverHandler.ClientGameOver -= ClientHandleGameOver;
    }
    private void Update(){

        MoveCommand();  

        if(Input.GetKeyUp(KeyCode.A))
        {
            if(autoAttackingMode) {
                autoAttackingMode = false;
                autoAttackMode.SetActive(false);
            }
            else{
                autoAttackingMode = true;
                autoAttackMode.SetActive(true);
            }
        }

        AutoAttack();

        
    }

    private void MoveCommand()
    {
        if(!Mouse.current.rightButton.wasPressedThisFrame){ return; }


        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)){ return; }


        if(autoAttackingMode){
            TryMove(hit.point);
            attackNearest = true;
        }
        else{
            if(hit.collider.TryGetComponent<Targetable>(out Targetable target))
            {
                if(target.hasAuthority)// my unit
                {
                    TryMove(hit.point);
                    return;
                }
                TryTarget(target);
                return;
            }
            TryMove(hit.point);
            attackNearest = false;
        }
        autoAttackingMode = false;
        autoAttackMode.SetActive(false);
    }

    private void AutoAttack()
    {
       if(!attackNearest){ return; }

       foreach(Unit unit in unitSelectionHandler.SelectedUnits)
        {
            //units = Physics.OverlapSphere(transform.position, sabotageRadius, unitLayer);
        }   
    }
    

    private void TryMove(Vector3 point)
    {
        foreach(Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMovement().CmdMove(point);
        }
        if(unitSelectionHandler.SelectedUnits.Count == 0){ return; }
        StartCoroutine(SpawnMoveObject(point));
    }
    IEnumerator SpawnMoveObject(Vector3 point)
    {
        GameObject tempObject = Instantiate(moveObject, point, moveObject.transform.rotation);
        yield return new WaitForSeconds(0.5f);
        Destroy(tempObject);
    }


    private void TryTarget(Targetable target)
    {
        foreach(Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetTargeter().CmdSetTarget(target.gameObject);
        }
    }

    private void ClientHandleGameOver(string winner){
        enabled = false;
    }
}
