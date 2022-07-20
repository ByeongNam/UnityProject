using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] private Stat stat = null;
    [SerializeField] private int PerResources = 1;
    [SerializeField] private float interval = 100f;

    private float timer;
    private GamePlayer player;

    public override void OnStartServer()
    {
        timer = interval;
        player = connectionToClient.identity.GetComponent<GamePlayer>();
        stat.CheckServerDie += ServerHandleDie;
        GameOverHandler.ServerGameOver += ServerHandleGameOver;
    }
    public override void OnStopServer()
    {
        stat.CheckServerDie -= ServerHandleDie;
        GameOverHandler.ServerGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update() {
        
        timer -= Time.deltaTime;

        if(timer <= 0){
            if(player != null){
                player.SetResources(player.GetResources() + PerResources);
                timer = interval;
            }
        }
    }

    private void ServerHandleDie(){
        NetworkServer.Destroy(gameObject);
    }
    private void ServerHandleGameOver(){
        enabled = false;
    }   
    

}
