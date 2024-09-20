using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerDataNetworked : NetworkBehaviour
{

    private const int STARTING_LIVES = 3;
    private const float INVULNERABILITY_DURATION = 2f; // Duração da invulnerabilidade em segundos
    private const float BLINK_INTERVAL = 0.2f; // Intervalo de piscar

    [Networked]
    public int Lives { get; private set; }

    [Networked]
    private bool spriteVisible { get; set; } = true; // Inicializa como true para garantir que o sprite comece visível

    private bool isInvulnerable = false;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer não encontrado!");
        }
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Lives = STARTING_LIVES;
            spriteVisible = true; // Garante que o sprite esteja visível no começo
        }
    }

    // Método para o player tomar dano
    public void SubtractLife()
    {
        if (!isInvulnerable)
        {
            // Envia um RPC para começar o processo de dano e invulnerabilidade
            RPC_TakeDamage();
        }
    }

    // RPC que é chamado quando o player toma dano
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_TakeDamage()
    {
        if (!isInvulnerable)
        {
            Lives--;

            if (Lives > 0)
            {
                // Inicia a corrotina de invulnerabilidade (piscar)
                StartCoroutine(BecomeInvulnerable());
            }
            else
            {
                Die();
            }
        }
    }

    // Corrotina para tornar o jogador invulnerável por um tempo e fazê-lo piscar
    private IEnumerator BecomeInvulnerable()
    {
        isInvulnerable = true;
        float elapsedTime = 0f;

        while (elapsedTime < INVULNERABILITY_DURATION)
        {
            // Alterna a visibilidade do sprite
            spriteVisible = !spriteVisible;
            yield return new WaitForSeconds(BLINK_INTERVAL);
            elapsedTime += BLINK_INTERVAL;
        }

        // Garante que o sprite fique visível após a invulnerabilidade
        spriteVisible = true;
        isInvulnerable = false;
    }

    public void Die()
    {
        Runner.Despawn(Object);
    }

    public override void FixedUpdateNetwork()
    {
        // Sincroniza a visibilidade do sprite manualmente para todos os clientes
        if (_spriteRenderer != null && _spriteRenderer.enabled != spriteVisible)
        {
            _spriteRenderer.enabled = spriteVisible;
        }
    }
}