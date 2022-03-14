using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    enum ProjectileType {Melee, Curve, Straight}
    
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private ProjectileType projectileType;
    [SerializeField] private float destroyAfterSeconds = 5f;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private int projectileDamage = 10;
    private void Start() 
    {
        if(projectileType == ProjectileType.Straight 
        || projectileType == ProjectileType.Melee)
        {
            rb.useGravity = false;
            rb.velocity = transform.forward * launchForce;
        }
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<NetworkIdentity>(out NetworkIdentity netId))
            if(netId.connectionToClient == connectionToClient){ return; } // self attack 방지
        
        if(other.TryGetComponent<Stat>(out Stat stat)){
            stat.DealDamage(projectileDamage);
        }
        if(projectileType == ProjectileType.Melee) { return; }// melee의 범위공격
        DestroySelf();
    }
}
