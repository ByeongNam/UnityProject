using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SabotageHandler : NetworkBehaviour
{
    [SerializeField] int unitRequirementNumberToSabotage = 5;
    [SerializeField] float sabotageRadius = 2.5f;
    [SerializeField] Collider[] units;
    [SerializeField] LayerMask unitLayer = new LayerMask();
    [SerializeField] TMP_Text neutralBuildingInfo = null;
    
    private int currentPeaceKeeperCount = 0;
    private int currentInfectorCount = 0;
    private bool sabotageEnable = true;

    
    private void Start() {
        neutralBuildingInfo.text = 
        $"It looks like we need <color=#8B0000><size=24>{ unitRequirementNumberToSabotage.ToString() }</size></color> people to take over this building";
    }
    private void Update() 
    {
        units = Physics.OverlapSphere(transform.position, sabotageRadius, unitLayer);
    }

    IEnumerator DelayForSabotage()
    {
        yield return new WaitForSeconds(3);
        sabotageEnable = true;
    }

    public bool RemoveSabotageUnit()
    {
        if(!sabotageEnable) { return false; } // sabotage cooltime 출력
        
        if(!hasAuthority){ return false; }

        foreach (Collider unit in units)
        {
            UnitPeaceKeeper peaceKeeper = unit.gameObject.GetComponent<UnitPeaceKeeper>();
            UnitInfector infector = unit.gameObject.GetComponent<UnitInfector>();

            if(peaceKeeper){
                currentPeaceKeeperCount++;
            }
            if(infector){
                currentInfectorCount++;
            }
        }

        if(currentPeaceKeeperCount == 0 && currentInfectorCount == 0){ 
            Debug.Log("no unit in area");
            return false; 
        } // 경고음

        sabotageEnable = false;
        StartCoroutine(DelayForSabotage());

        if(currentPeaceKeeperCount > currentInfectorCount){
            int objectCount = currentPeaceKeeperCount - unitRequirementNumberToSabotage;
            if(objectCount < 0){
                Debug.Log("not enough unit");
                return false;
            }
            else{
                foreach (Collider unit in units)
                {
                    UnitPeaceKeeper peaceKeeper = unit.gameObject.GetComponent<UnitPeaceKeeper>();;

                    if(peaceKeeper){
                        currentPeaceKeeperCount--;
                        unit.gameObject.GetComponent<Stat>().DealDamage(-1); // sabotage die
                    }
                    if(objectCount==currentPeaceKeeperCount){ break; }
                }
                Debug.Log("PeaceKeeper Sabotage");
                return true;
            }
            
        }
        else{
            // 빌딩위에 전투중 뜨게
            return false;
        }
    }

    public void RemoveBuilding(){
        gameObject.GetComponent<Stat>().DealDamage(-1);
    }
}
