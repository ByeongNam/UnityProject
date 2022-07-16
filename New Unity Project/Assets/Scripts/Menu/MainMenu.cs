using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject startPagePanel = null;
    [SerializeField] private GameObject landingPagePanel = null;

    private void Start() 
    {
        startPagePanel.SetActive(true);
    }
    public void StartPage()
    {
        if(startPagePanel.activeSelf == false){ return; }

        // adding Camera movement  

        startPagePanel.SetActive(false);
        
        StartCoroutine(LandingPageActive());
    }

    IEnumerator LandingPageActive(){
        yield return new WaitForSeconds(1);
        landingPagePanel.SetActive(true);
    }
    public void HostLobby()
    {
        // adding Camera movement   
        landingPagePanel.SetActive(false);

        NetworkManager.singleton.StartHost(); // start host in mirror
    }
}
