using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private UnitAnimation unitAnimation = null;
    [SerializeField] private Stat stat = null;

    private bool isAttacking = false;

    public override void OnStartServer()
    {
        GameOverHandler.ServerGameOver += ServerHandleGameOver;
        stat.CheckServerDie += HandleServerDie;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerGameOver -= ServerHandleGameOver;
        stat.CheckServerDie -= HandleServerDie;
    }
    public void StartAttacking(){
        isAttacking = true;
        StartCoroutine(DelayMovement());
    }

    IEnumerator DelayMovement(){
        yield return new WaitForSeconds(unitAnimation.GetAttackAnimationLength()-0.2f); // 0.2f 는 선입력
        isAttacking = false;
    }
    #region Server
    
    [ServerCallback]
    private void Update()// agent 중지 
    { 
        Targetable target = targeter.GetTarget();
        
        if(target != null)// chasing
        {
            Vector3 targetPosition = target.transform.position;
            Vector3 myPosition = transform.position;

            if((targetPosition - myPosition).sqrMagnitude > chaseRange * chaseRange){
                // 정확한 거리 측정이 아닌 길이비교시 효율성 좋은 방법
                agent.SetDestination(target.transform.position);
            }
            else if(agent.hasPath){
                agent.ResetPath();
            }
            return;
        }

        if(!agent.hasPath){ return; }

        if(agent.remainingDistance > agent.stoppingDistance){ return; }
        
        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }

    [Server]
    public void ServerMove(Vector3 position)
    {
        if(isAttacking){ return; }

        targeter.ClearTarget();
        
        if(!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)){ return; }
        //해당 soucePosition에 maxDistance의 구체를 생성해서 NavMesh가 있는지 체크, 체크해서 반환하는 변수가 NavMeshHit 
        agent.SetDestination(hit.position);

    }

    [Server]
    private void ServerHandleGameOver()
    {
        agent.enabled = false;
    }

    [Server]
    private void HandleServerDie(){
        agent.enabled = false;
    }
    
    #endregion

    #region Client 

    #endregion

}
