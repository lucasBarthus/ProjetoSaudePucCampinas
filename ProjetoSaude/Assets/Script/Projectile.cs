using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Projectile : NetworkBehaviour
{
    private PlayerMovementFusion owner; // Referência ao jogador que disparou o projétil
    [SerializeField] private int pointsForKill = 50;  // Pontos por destruir um inimigo
    public NetworkObject NetworkObject;

    public void SetOwner(PlayerMovementFusion player)
    {
        owner = player;
    }

    private void Awake()
    {
        NetworkObject = GetComponent<NetworkObject>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("Projétil colidiu com o inimigo");

            if (owner != null)
            {
                owner.OnEnemyHit(pointsForKill);
                // Notifica o servidor que um inimigo foi atingido
                owner.RpcDestroyBullet(NetworkObject);
                Debug.Log("Notificando o servidor que o inimigo foi atingido");
            }
        }
    }
}