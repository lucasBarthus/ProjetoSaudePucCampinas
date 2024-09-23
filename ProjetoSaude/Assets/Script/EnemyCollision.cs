using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollision : NetworkBehaviour
{
    [SerializeField] private int vida = 1; // Vida inicial do inimigo

    public NetworkObject NetworkObject { get; private set; }

    private void Awake()
    {
        NetworkObject = GetComponent<NetworkObject>();  
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            if (Object.HasStateAuthority)
            {
                RPC_EnemyHit("Bullet");
            }

            // Despawns o projétil se o cliente tiver autoridade
            if (collision.TryGetComponent<NetworkObject>(out var networkObject))
            {
                if (networkObject.HasStateAuthority)
                {
                    Runner.Despawn(NetworkObject);
                }
            }
        }
        else if (collision.CompareTag("Wall"))
        {
            if (Object.HasStateAuthority)
            {
                Runner.Despawn(NetworkObject);
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_EnemyHit(string source)
    {
        vida--; // Reduz a vida do inimigo
        Debug.Log($"O inimigo foi atingido por {source}! Vida restante: {vida}");

        if (vida < 1)
        {
            if (TryGetComponent<NetworkObject>(out var networkObject))
            {
                if (networkObject.HasStateAuthority)
                {
                    var enemySpawner = FindObjectOfType<EnemySpawner>();
                    if (enemySpawner != null)
                    {
                        enemySpawner.RpcDestroyEnemy(networkObject); // Chama o RPC para destruir o inimigo
                    }
                    else
                    {
                        Debug.LogWarning("EnemySpawner não encontrado.");
                    }
                }
            }

            Debug.Log($"O inimigo foi destruído!");
        }
    }
}