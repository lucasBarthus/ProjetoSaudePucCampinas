using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Projectile : NetworkBehaviour
{
    private PlayerMovementFusion owner; // Referência ao jogador que disparou o projétil
   
    public void SetOwner(PlayerMovementFusion player)
    {
        owner = player;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("Projétil colidiu com o inimigo");

            // Obtém o NetworkObject do inimigo
            NetworkObject enemyObject = collision.GetComponent<NetworkObject>();

            if (enemyObject != null && owner != null)
            {
                
              

                // Destrói o projétil
                owner.RpcDestroyBullet(Object); // Usa 'Object' para referenciar o NetworkObject atual
                Debug.Log("Projétil destruído");
            }
        }
    }
    public PlayerMovementFusion GetOwner()
    {
        return owner;
    }
}
