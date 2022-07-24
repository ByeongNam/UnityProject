using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNetworkManager : NetworkManager
{
    [SerializeField] private GameOverHandler gameOverHandler = null;
    [SerializeField] private GameObject unitBasePrefab = null;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    public static event Action OnStartGame;

    private bool isGameInProgress = false;

    public List<GamePlayer> Players { get; } = new List<GamePlayer>();

    #region Server
    public override void OnServerConnect(NetworkConnection conn)
    {
        if(!isGameInProgress) { return; }

        conn.Disconnect();
    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        GamePlayer player = conn.identity.GetComponent<GamePlayer>();

        Players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();

        isGameInProgress = false;
    }

    public void StartGame(){
        if(Players.Count < 2){ return; }
        
        isGameInProgress = true;

        OnStartGame?.Invoke();

        ServerChangeScene("Scene_Map_01"); // 맵 이름 input으로 변경
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        GamePlayer player = conn.identity.GetComponent<GamePlayer>();

        Players.Add(player);

        player.SetDisplayName($"Player {Players.Count}");

        player.SetIsPartyOwner(Players.Count == 1); // 처음 서버에 추가된 플레이어에게 partyowner 부여
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if(SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        { // 맵 scene 시작시
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandler);

            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach(GamePlayer player in Players){
                GameObject baseInstance = Instantiate(
                    unitBasePrefab,
                    GetStartPosition().position, 
                    Quaternion.identity);

                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }
        }
    }

    #endregion

    #region Client
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        
        ClientOnConnected?.Invoke();
    }
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        ClientOnDisconnected?.Invoke();
    }
    public override void OnStopClient()
    {
        Players.Clear();
    }


    #endregion

    
    
}
