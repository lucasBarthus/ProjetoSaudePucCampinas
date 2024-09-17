using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Projectile : NetworkBehaviour
{
    private PlayerMovementFusion owner; // Referência ao jogador que disparou o projétil
    [SerializeField] private int pointsForKill = 50;  // Pontos por destruir um inimigo

    public void SetOwner(PlayerMovementFusion player)
    {
        owner = player;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se o projétil colidiu com um inimigo
        if (collision.CompareTag("Enemy"))
        {
            // Chama o método OnEnemyHit no jogador para adicionar pontos
            if (owner != null)
            {
                owner.OnEnemyHit(pointsForKill);
            }
            // Destrói o inimigo
            Runner.Despawn(collision.GetComponent<NetworkObject>());

            // Destrói o projétil
            Runner.Despawn(Object);
        }
    }


}
