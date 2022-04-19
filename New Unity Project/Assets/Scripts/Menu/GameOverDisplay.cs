using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] private GameObject gameOverDisplay = null;
    [SerializeField] private TMP_Text winnerText = null;
    private void Start() 
    {
        GameOverHandler.ClientGameOver += ClientHandleGameOver;
    }

    private void OnDestroy() 
    {
        GameOverHandler.ClientGameOver -= ClientHandleGameOver;
    }

    private void ClientHandleGameOver(string winner)
    {
        winnerText.text = $"{winner} Win!";
        StartCoroutine(ActiveGameOverDisplay());
    }
    IEnumerator ActiveGameOverDisplay(){
        yield return new WaitForSeconds(3);
        gameOverDisplay.SetActive(true);
    }

    public void LeaveGame(){
        if(NetworkServer.active && NetworkClient.isConnected){
            NetworkManager.singleton.StopHost(); 
            // singleton 어플리케이션이 시작될 때 어떤 클래스가 최초 한번만 메모리를 할당하고(static) 그 메모리에 인스턴스를 만들어 사용하는 디자인 패턴이다.
        }
        else{
            NetworkManager.singleton.StopClient();
        }
    }
}
