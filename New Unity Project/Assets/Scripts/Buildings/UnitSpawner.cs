using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;

    

    #region Server

    [Command]
    private void CmdSpawnUnit(){
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
        if(eventData.button != PointerEventData.InputButton.Left){ return; }
        
        if(!hasAuthority){ return; }

        CmdSpawnUnit();
    }
    
    #endregion
}
