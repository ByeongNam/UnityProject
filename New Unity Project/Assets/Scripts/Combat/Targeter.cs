using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    private Targetable target;

    public Targetable GetTarget()
    {
        return target;
    }
    public override void OnStartServer()
    {
        GameOverHandler.ServerGameOver += ServerHandleGameOver;
    }
    public override void OnStopServer()
    {
        GameOverHandler.ServerGameOver -= ServerHandleGameOver;
    }
    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        if(targetGameObject == null){ return; }
        if(!targetGameObject.TryGetComponent<Targetable>(out Targetable outTarget)){ return; } 
        // targetable cs 유무 확인

        target = outTarget;
    }
    
    [Server]
    public void ClearTarget()
    {
        target = null;
    }

    [Server]
    private void ServerHandleGameOver(){
        ClearTarget();
    }

}
