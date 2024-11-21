using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class PowerUpVida : NetworkBehaviour, IPowerUp
{
    public int vidaRestaurada = 1; // Quantidade de vida que o Power-up ir� restaurar

    public void OnPickup(NetworkObject player)
    {
        // Verifica se o jogador possui o componente PlayerMovementFusion
        if (player.TryGetComponent<PlayerMovementFusion>(out var playerMovement))
        {
            // Chama o m�todo AddLives no script do jogador
            playerMovement.AddLives(vidaRestaurada);
            Debug.Log($"Power-up de Vida: {vidaRestaurada} vida restaurada para {playerMovement}.");
        }
        else
        {
            Debug.LogWarning("O jogador n�o tem o componente PlayerMovementFusion.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se o objeto colidido tem a tag "Player"
        if (collision.CompareTag("Player"))
        {
            // Obt�m o componente NetworkObject para o player
            if (collision.TryGetComponent<NetworkObject>(out var player))
            {
                // Chama o m�todo OnPickup passando o jogador como argumento
                OnPickup(player);

                // Despawner o Power-up ap�s ser coletado
                Runner.Despawn(GetComponent<NetworkObject>());
            }
        }
    }
}