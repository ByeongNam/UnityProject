using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class UnitAttack: NetworkBehaviour
{
    enum AttackType {Melee, Range};
    
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private UnityEvent triggerAttackAnimation = null;
    [Header("Unit Traits")]
    [SerializeField] private AttackType unitAttackType;
    [SerializeField] private float fireRange = 10f;
    [SerializeField] private float attackSpeed = 1;
    [SerializeField] private float rotationSpeed = 20f;

    Vector3 targetPosition;

    private float lastFireTime;

    [ServerCallback]
    private void Update(){
        if(targeter.GetTarget() == null){ return; }

        if(!CanAttackAtTarget()){ return; }

        if(agent.velocity.magnitude > 0.1f){ return; }

        Quaternion targetRotation = 
            Quaternion.LookRotation(targeter.GetTarget().transform.position - transform.position);;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime); // adjusting firing angle 
        
        if(Time.time > (1 / attackSpeed) + lastFireTime) // 공격속도
        {   
            triggerAttackAnimation?.Invoke();
            
            Quaternion projectileRotation = Quaternion.LookRotation(
                targeter.GetTarget().GetAimAtPoint().position - projectileSpawnPoint.position); 
                // target의 에임 포인트위치와 투사체 위치가 이루는 방향

            GameObject projectileInstance = Instantiate(
                projectilePrefab, projectileSpawnPoint.position, projectileRotation);

            NetworkServer.Spawn(projectileInstance, connectionToClient);

            lastFireTime = Time.time;
        }
    }

    [Server]
    private bool CanAttackAtTarget(){
        return (Vector3.Distance(targeter.GetTarget().transform.position, transform.position) 
            <= fireRange); // distance sqrmagnitude 대신
    }
}
