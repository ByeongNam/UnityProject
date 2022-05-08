using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralBuildingPointId : MonoBehaviour
{
    [SerializeField] [Range(1,10)]int id = -1;

    public int GetNeutralBuildingId(){
        return id;
    }
}
