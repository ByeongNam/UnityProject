using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitAnimation : NetworkBehaviour
{   
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Animator unitAnimator = null;
    [SerializeField] private NetworkAnimator networkAnimator = null;
    [SerializeField] private Stat stat = null;

    bool isAttacking = false;

    public override void OnStartServer()
    {
        stat.CheckServerDie += ServerHandleDieAnimation;
    }
    public override void OnStopServer()
    {
        stat.CheckServerDie -= ServerHandleDieAnimation;
    }
    public float GetAttackAnimationLength()
    {
        return unitAnimator.GetCurrentAnimatorStateInfo(0).length;
    }
    public void TriggerIsAttacking()
    {
        isAttacking = true;
    }

    #region Server
    [ServerCallback]
    private void Update()
    {

        if(isAttacking)
        {
            isAttacking = false;
            networkAnimator.SetTrigger("Attack");
        }
        
        unitAnimator.SetBool(
            "Move",agent.velocity.magnitude > 0.4f);// Move animation
    }
    [Server]
    public void ServerHandleDieAnimation(){
        networkAnimator.SetTrigger("Die");
    }
    #endregion
    
}
