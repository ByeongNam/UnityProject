using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Stat : NetworkBehaviour
{
    [SerializeField] private int healthPoint = 100;
    [Range(0,10)] 
    [SerializeField] private int defensivePower = 1;
    [Range(0,50)] 
    [SerializeField] private int blockRatio = 10;
 
    public event Action CheckServerDie;
    public event Action<int ,int> ClientOnHealthUpdated;

    [SyncVar(hook = nameof(HPbarUpdate))] //서버에서 각 클라이언트로 동기화, 네트워크 오브젝트 상의 모든 SyncVars 최신 상태가 전송
    private int currentHP;

    #region Server

    public override void OnStartServer()
    {
        currentHP = healthPoint;
    }

    [Server]
    public void DealDamage(int deal){
        
        if(currentHP == 0) { return; }

        if(UnityEngine.Random.Range(1,100) <= blockRatio){
            Debug.Log("Blocked!");
            currentHP -= 0; // blocked
        }
        else{
            Debug.Log("Hit!");
            float decreasedRatio =  1f - ((defensivePower * 10f) / 100f);
            currentHP -= (int)((float)deal * decreasedRatio); // 방어력 감산
        }

        if(currentHP < 0) currentHP = 0;

        Debug.Log(currentHP);

        if(currentHP != 0) { return; }

        CheckServerDie?.Invoke();
    }

    #endregion

    #region Client

    private void HPbarUpdate(int oldHP, int newHP){
        ClientOnHealthUpdated?.Invoke(newHP, healthPoint);
    }

    #endregion
}
