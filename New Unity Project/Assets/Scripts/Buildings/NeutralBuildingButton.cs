using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NeutralBuildingButton : MonoBehaviour
{
    [SerializeField] private NeutralBuilding nbuilding = null;
    [SerializeField] private SabotageHandler sabotageHandler = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text functionTextUnit = null;

    private GamePlayer player;

    private void Start() 
    {
        iconImage.sprite = nbuilding.GetUnitIcon();

        if(functionTextUnit == null) { return; }

        functionTextUnit.text = "hi";
    }

    private void Update() 
    {
        if(player == null){
            player = NetworkClient.connection.identity.GetComponent<GamePlayer>();
        }
        if(Input.GetKeyDown(KeyCode.G)){
            OnGKeyDown();
        }
        if(Input.GetKeyUp(KeyCode.G)){
            OnGKeyUp();
        }
    }
    public void OnGKeyDown()
    {
        
    }

    public void OnGKeyUp()
    {
        if(!sabotageHandler.RemoveSabotageUnit()) { return; }
        
        player.CmdPlaceBuilding(nbuilding.GetId(), nbuilding.gameObject.transform.position); // 건물생성이펙트 여기에
        sabotageHandler.RemoveBuilding();
        
    }
}
