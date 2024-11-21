using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class PowerUpVelocidade : NetworkBehaviour, IPowerUp
{
    public float aumentoVelocidade = 2f; // Aumento na velocidade do jogador
    public float duracaoBoost = 5f; // Duração do boost de velocidade em segundos

    public void OnPickup(NetworkObject player)
    {
        if (player.TryGetComponent<PlayerMovementFusion>(out var playerMovement))
        {
            // Aumenta a velocidade do jogador e define a duração do boost
            playerMovement.IncreaseSpeed(aumentoVelocidade, duracaoBoost);

           

            // Despawner o Power-up após ser coletado
            Runner.Despawn(GetComponent<NetworkObject>());
        }
        else
        {
            
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se o objeto colidido tem a tag "Player"
        if (collision.CompareTag("Player"))
        {
            // Obtém o componente NetworkObject para o player
            if (collision.TryGetComponent<NetworkObject>(out var player))
            {
                // Chama o método OnPickup passando o jogador como argumento
                OnPickup(player);
                
            }
        }
    }
}