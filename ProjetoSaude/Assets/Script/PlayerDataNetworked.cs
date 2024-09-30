using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerDataNetworked : NetworkBehaviour
{

    private const int STARTING_LIVES = 3;
    private const float INVULNERABILITY_DURATION = 2f;
    private const float BLINK_INTERVAL = 0.2f;
    public bool IsDead { get; private set; } = false;

    [Networked]
    public int Lives { get; private set; }

    [Networked]
    private bool spriteVisible { get; set; } = true;

    private bool isInvulnerable = false;
    private SpriteRenderer _spriteRenderer;
    private bool hasNotifiedReadyManager = false;
    private bool notifiedReadyManager = false; // Variável para garantir que só notifique uma vez

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
            spriteVisible = true;
        }
    }

    public void SubtractLife()
    {
        if (!isInvulnerable)
        {
            RPC_TakeDamage();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_TakeDamage()
    {
        if (!isInvulnerable)
        {
            Lives--;

            if (Lives > 0)
            {
                StartCoroutine(BecomeInvulnerable());
            }
            else
            {
                Die();
            }
        }
    }

    private IEnumerator BecomeInvulnerable()
    {
        isInvulnerable = true;
        float elapsedTime = 0f;

        while (elapsedTime < INVULNERABILITY_DURATION)
        {
            spriteVisible = !spriteVisible;
            yield return new WaitForSeconds(BLINK_INTERVAL);
            elapsedTime += BLINK_INTERVAL;
        }

        spriteVisible = true;
        isInvulnerable = false;
    }

    private void Die()
    {
        if (IsDead) return; // Já está morto

        IsDead = true;

        if (!hasNotifiedReadyManager)
        {
            NotifyReadyManager();
            hasNotifiedReadyManager = true; // Garante que o ReadyManager só seja notificado uma vez
        }

        Runner.Despawn(Object);
    }
    public override void FixedUpdateNetwork()
    {
        if (_spriteRenderer != null && _spriteRenderer.enabled != spriteVisible)
        {
            _spriteRenderer.enabled = spriteVisible;
        }
    }

    private void NotifyReadyManager()
    {
        ReadyManager.Instance.CheckPlayersDead();
    }
}