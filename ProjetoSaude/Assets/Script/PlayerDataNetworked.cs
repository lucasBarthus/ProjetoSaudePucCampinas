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

    private bool isInvulnerable = false;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        // Obtém o SpriteRenderer do jogador para controlar sua visibilidade
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Lives = STARTING_LIVES;
        }
    }

    public void SubtractLife()
    {
        if (!isInvulnerable)
        {
            Lives--;

            StartCoroutine(BecomeInvulnerable());

            if (Lives <= 0)
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

        // Enquanto o jogador estiver invulnerável, ele vai piscar
        while (elapsedTime < INVULNERABILITY_DURATION)
        {
            _spriteRenderer.enabled = !_spriteRenderer.enabled; // Alterna a visibilidade
            yield return new WaitForSeconds(BLINK_INTERVAL); // Espera um intervalo para piscar
            elapsedTime += BLINK_INTERVAL;
        }

        // Garante que o sprite fique visível após a invulnerabilidade
        _spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    public void Die()
    {
        gameObject.SetActive(false);
    }
}