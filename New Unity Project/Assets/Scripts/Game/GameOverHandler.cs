using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action ServerGameOver;
    public static event Action<string> ClientGameOver; // static 프로그램 시작시 메모리에 올라가 프로그램 종료시 할당 해제
    // 채팅 메세지 보낼때 활용?
    private List<UnitBase> bases = new List<UnitBase>();

    #region Server
    public override void OnStartServer()
    {
        UnitBase.ServerBaseSpawned += ServerHandleBaseSpawned; // UnitBase 따로 선언x -> Static
        UnitBase.ServerBaseDespawned += ServerHandleBaseDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerBaseDespawned -= ServerHandleBaseDespawned;
    }
    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }
    [Server]
    private void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);

        if(bases.Count != 1) { return; }

        int playerId = bases[0].connectionToClient.connectionId; // user name 으로 변경 고려
        string playerString = $"Peace Keeper ( ID : {playerId})";
        if (playerId != 0)
            playerString = $"Infector ( ID : {playerId})";

        RpcGameOver(playerString); // 문자열 보간 {} 안에 변수입력

        ServerGameOver?.Invoke();
    }
    #endregion

    #region Client

    [ClientRpc] // remote procedure call
    private void RpcGameOver(string winner)
    {
        ClientGameOver?.Invoke(winner);
    }

    #endregion
}
