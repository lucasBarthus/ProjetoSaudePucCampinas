using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class PlayerCollisionHandler : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se a colisão foi com um inimigo
        if (other.CompareTag("Enemy"))
        {
            // Obtém o NetworkObject do objeto com o qual colidiu
            NetworkObject enemyNetworkObject = other.GetComponent<NetworkObject>();

            // Se o NetworkObject for encontrado, processa a colisão
            if (enemyNetworkObject != null)
            {
                // Se for o Host, lida com a colisão diretamente
                if (Object.HasStateAuthority)
                {
                    HandleCollision(enemyNetworkObject);
                }
                else
                {
                    // Se for um cliente, envia um RPC para o Host
                    RPC_HandleCollision(enemyNetworkObject);
                }
            }
        }
    }

    // Método local para lidar com a colisão no Host
    private void HandleCollision(NetworkObject enemyNetworkObject)
    {
        Debug.Log($"Colisão detectada entre o Player e o Enemy: {enemyNetworkObject.name} pelo Host.");

        // Implementar lógica adicional aqui, como reduzir a vida, aplicar dano, etc.
    }

    // RPC enviado por um cliente e processado pelo Host
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_HandleCollision(NetworkObject enemyNetworkObject)
    {
        Debug.Log($"Colisão detectada entre o Player e o Enemy: {enemyNetworkObject.name} por um Cliente.");

        // Lógica de colisão é executada no Host
        HandleCollision(enemyNetworkObject);
    }
}