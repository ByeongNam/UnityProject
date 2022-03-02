using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Animator unitAnimator = null;
    private Camera mainCamera;

    #region Server

    [Command]
    private void CmdMove(Vector3 position)
    {
        if(!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)){return;}
        //해당 soucePosition에 maxDistance의 구체를 생성해서 NavMesh가 있는지 체크, 체크해서 반환하는 변수가 NavMeshHit 
        agent.SetDestination(hit.position);
    }
    [Command]
    private void CmdAnimation()
    {
        unitAnimator.SetBool(
            "Move",agent.velocity.magnitude > 0.2f);// animation
    }

    #endregion

    #region Client 

    public override void OnStartAuthority()
    {
        mainCamera = Camera.main;
    }

    [ClientCallback] // Unity에서 자동으로 호출하는 빌트인 콜백 함수에 사용 경고 생성 안됨
    private void Update()
    {
        

        if(!hasAuthority){return;}
        // hasAuthority 프로퍼티를 사용하여 오브젝트에 권한이 있는지 파악할 수 있다. 
        // 비플레이어 오브젝트는 서버에 권한이 있고,
        // 플레이어 오브젝트 중 localPlayerAuthority가 설정된 것은 각 소유자 클라이언트에 권한이 있다.

        CmdAnimation(); // Send animation related info to Server

        if(!Mouse.current.rightButton.wasPressedThisFrame){return;}// 우클릭(Input System)
        
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()); // 마우스위치(Input System)

        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)){return;}
        // 레이캐스트를 사용하면 광선에 충돌되는 콜라이더(Collider)에 대한 거리, 위치 등의 자세한 정보를 RaycastHit로 반환

        CmdMove(hit.point);
    }

    #endregion
}
