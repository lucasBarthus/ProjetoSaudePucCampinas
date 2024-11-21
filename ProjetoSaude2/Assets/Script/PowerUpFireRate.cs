using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class PowerUpFireRate : NetworkBehaviour, IPowerUp
{
    public float aumentoFireRate = 0.5f; // Aumento no Fire Rate
    public float duracaoBoost = 5f;
    public void OnPickup(NetworkObject player)
    {

        if (player.TryGetComponent<PlayerMovementFusion>(out var playerMovement))
        {
            // Aumenta a velocidade do jogador e define a dura��o do boost
            playerMovement.IncreaseFireRate(aumentoFireRate, duracaoBoost);



            // Despawner o Power-up ap�s ser coletado
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
            // Obt�m o componente NetworkObject para o player
            if (collision.TryGetComponent<NetworkObject>(out var player))
            {
                // Chama o m�todo OnPickup passando o jogador como argumento
                OnPickup(player);

                // Despawner o Power-up ap�s ser coletado
               
            }
        }
    }
}