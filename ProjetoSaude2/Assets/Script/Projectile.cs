using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Projectile : NetworkBehaviour
{
    private PlayerMovementFusion owner; // Refer�ncia ao jogador que disparou o proj�til
   
    public void SetOwner(PlayerMovementFusion player)
    {
        owner = player;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("Proj�til colidiu com o inimigo");

            // Obt�m o NetworkObject do inimigo
            NetworkObject enemyObject = collision.GetComponent<NetworkObject>();

            if (enemyObject != null && owner != null)
            {
                
              

                // Destr�i o proj�til
                owner.RpcDestroyBullet(Object); // Usa 'Object' para referenciar o NetworkObject atual
                Debug.Log("Proj�til destru�do");
            }
        }
    }
    public PlayerMovementFusion GetOwner()
    {
        return owner;
    }
}
