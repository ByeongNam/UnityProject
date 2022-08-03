using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ObjectMaterializeHandler : NetworkBehaviour
{
    [SerializeField] private int id = 0;

    public int GetId()
    {
        return id;
    }
}
