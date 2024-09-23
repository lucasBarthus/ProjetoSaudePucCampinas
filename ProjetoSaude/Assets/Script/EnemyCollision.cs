using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollision : NetworkBehaviour
{
    [SerializeField] private int vida = 1; // Vida inicial do inimigo

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            if (Object.HasStateAuthority)
            {
                RPC_EnemyHit("Bullet");
            }

            if (collision.TryGetComponent<NetworkObject>(out var networkObject))
            {
                if (networkObject.HasStateAuthority)
                {
                    Runner.Despawn(networkObject); // Despawns o projétil
                }
            }
        }

        if (collision.CompareTag("Wall"))
        {
            if (Object.HasStateAuthority)
            {
                RPC_EnemyHit("Wall");
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_EnemyHit(string source)
    {
        vida--;
        Debug.Log($"O inimigo foi atingido por {source}! Vida restante: {vida}");

        if (vida < 1)
        {
            if (TryGetComponent<NetworkObject>(out var networkObject))
            {
                // Retorna o inimigo à pool
                if (networkObject.HasStateAuthority)
                {
                    var enemySpawner = FindObjectOfType<EnemySpawner>();
                    enemySpawner.objectPool.ReleaseInstance(Runner, networkObject); // Retorne à pool
                }
            }

            Debug.Log($"O inimigo foi destruído!");
        }
    }
}