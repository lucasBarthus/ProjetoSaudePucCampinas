using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class PlayerCollisionHandler : NetworkBehaviour
{

    private PlayerDataNetworked _playerData;

    private void Start()
    {
        // Encontra o componente PlayerDataNetworked no objeto do jogador
        _playerData = GetComponent<PlayerDataNetworked>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se a colis�o foi com um inimigo
        if (other.CompareTag("Enemy"))
        {
            // Obt�m o NetworkObject do objeto com o qual colidiu
            NetworkObject enemyNetworkObject = other.GetComponent<NetworkObject>();

            // Se o NetworkObject for encontrado, processa a colis�o
            if (enemyNetworkObject != null)
            {
                // Se for o Host, lida com a colis�o diretamente
                if (Object.HasStateAuthority)
                {
                    HandleCollision();
                }
                else
                {
                    // Se for um cliente, envia um RPC para o Host
                    RPC_HandleCollision();
                }
            }
        }
    }

    // M�todo local para lidar com a colis�o no Host
    private void HandleCollision()
    {
       

        // Se o jogador tem um PlayerDataNetworked, subtrai uma vida
        if (_playerData != null)
        {
            _playerData.SubtractLife();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_HandleCollision()
    {
        HandleCollision();
    }
}