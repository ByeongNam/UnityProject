using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Unit : NetworkBehaviour // UnitSelectionHandler 에서 쓰임
{
    [SerializeField] private int resourceCost = 1;
    [SerializeField] private UnitMovement unitMovement = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private Stat stat = null;
    [SerializeField] private Targetable targetable = null;
    [SerializeField] private UnityEvent OnSelected = null;
    //Unity 에서 제공하는 event
    [SerializeField] private UnityEvent OnDeselected = null;
    [SerializeField] private Animator animator = null;
    [SerializeField] private Sprite unitIcon = null;
    [SerializeField] private Collider[] units = null;
    private float autoAttackRadius = 2.5f;
    private bool autoAttackDetection = false;
    [SerializeField] LayerMask targetLayer = new LayerMask();

    /*
    1. Event Listener가 2개 이상인 경우, UnityEvent가 C# Event에 비해 메모리를 덜 Allocation한다. (1개인 경우 그 반대)

    2. Event Dispatch의 경우 UnityEvent는 맨 처음 Dispatch할 때 가비지를 발생시킨다. C# Event는 가비지가 발생하지 않는다.

    3. UnityEvent는 C# Event에 비해 최소 두 배 느리고, worst case의 경우 40배까지 느렸다.
    */

    public static event Action<Unit> ServerUnitSpawned;
    public static event Action<Unit> ServerUnitDespawned;
    public static event Action<Unit> AuthorityUnitSpawned;
    public static event Action<Unit> AuthorityUnitDespawned;
    public static event Action<Unit> SelectedUnitDespawned;
    // event 는 delegate(대리자) 일종 
    // 이벤트는 개체에서 작업 실행을 알리기 위해 보내는 메시지
    
    public Sprite GetUnitIcon(){
        return unitIcon;
    }
    public int GetResourceCost(){
        return resourceCost;
    }
    public UnitMovement GetUnitMovement(){
        return unitMovement;
    }
    public Targeter GetTargeter(){
        return targeter;
    }

    public void StartAutoAttackDetection()
    {
        autoAttackDetection = true;
    }

    public void DisableAutoAttackDetection()
    {
        autoAttackDetection = false;
    }

    public AnimatorStateInfo GetCurrentAnimationInfo(){
        return animator.GetCurrentAnimatorStateInfo(0);
    }

    private void Start() 
    {
        OnDeselected?.Invoke();
    }
    private void Update() 
    {
        if(!autoAttackDetection)
        {
            units = null;
            return;
        }
        units = Physics.OverlapSphere(transform.position, autoAttackRadius, targetLayer);
    }

    public Targetable FindNearestTarget()
    {
        float minDistance = 99f;
        Targetable target = null;
        Targetable realTarget = null;
        if(units == null)
        {
            return null;
        }
        else
        {
            foreach (Collider unit in units)
            {
                float curDistance = Vector3.Distance(transform.position, unit.transform.position);
                if(minDistance > curDistance)
                {
                    minDistance = curDistance;
                    target = unit.gameObject.GetComponent<Targetable>();
                    if(target == null){ continue; }
                    if(target.hasAuthority){ continue; }
                    realTarget = target;
                }
            }
            return realTarget;
        }
    }

    #region Server

    public override void OnStartServer()
    {
        ServerUnitSpawned?.Invoke(this); // trigger (=this happened)
        // ? 는 null 검사
        stat.CheckServerDie += HandleServerDie;
        stat.CheckServerSabotageDie += HandleServerSabotageDie;
    }

    public override void OnStopServer()
    {
        ServerUnitDespawned?.Invoke(this);
        stat.CheckServerDie -= HandleServerDie;
        stat.CheckServerSabotageDie -= HandleServerSabotageDie;
    }

    [Server]
    private void HandleServerDie()
    {
        Destroy(targetable);
        StartCoroutine(DelayDeath()); // remove targeting
    }
    IEnumerator DelayDeath()
    {
        yield return new WaitForSeconds(2);
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void HandleServerSabotageDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        if(!hasAuthority){ return; } // server x
              
        AuthorityUnitSpawned?.Invoke(this);
    }
    public override void OnStopClient()
    {
        if(!hasAuthority){ return; }
        SelectedUnitDespawned?.Invoke(this);  
        AuthorityUnitDespawned?.Invoke(this);
    }

    [Client]
    public void Select()
    {
        if(!hasAuthority){ return; }
        
        OnSelected?.Invoke(); // sprite enable
    }

    [Client]
    public void Deselect()
    {
        if(!hasAuthority){ return; }

        OnDeselected?.Invoke(); // sprite disable
    }
    #endregion
}
