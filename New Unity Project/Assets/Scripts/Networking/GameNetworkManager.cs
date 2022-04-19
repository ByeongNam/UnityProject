using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNetworkManager : NetworkManager
{
    [SerializeField] private GameOverHandler gameOverHandler = null;
    [SerializeField] private GameObject unitSpawnerPrefab = null;
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        GameObject unitSpawnerInstance = Instantiate(
            unitSpawnerPrefab, 
            conn.identity.transform.position, 
            conn.identity.transform.rotation); // server 에 Instantiate

        NetworkServer.Spawn(unitSpawnerInstance, conn); // client 에 스폰
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if(SceneManager.GetActiveScene().name.StartsWith("Scene_Map")){ // 맵 scene 시작시
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandler);

            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
        }
    }
}
