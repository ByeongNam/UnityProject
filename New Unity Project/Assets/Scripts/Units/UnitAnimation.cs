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
        stat.CheckServerDie += HandleServerDieAnimation;
    }
    public override void OnStopServer()
    {
        stat.CheckServerDie -= HandleServerDieAnimation;
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
            "Move",agent.velocity.magnitude > 0.3f);// Move animation
    }
    [Server]
    public void HandleServerDieAnimation(){
        Debug.Log("Die animation");
    }
    #endregion
    
}
