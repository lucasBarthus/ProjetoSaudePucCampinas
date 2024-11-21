using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Globalization;
using TMPro;

public class PlayerMovementFusion : NetworkBehaviour
{

    public bool powerUpVelocity = false;
    public bool powerUpFireRate = false;


    private Rigidbody2D _rb;
    public float moveSpeed = 15f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float fireCooldown = 0.2f;
    private float fireCooldownInitial = 0.2f;
    [Networked] public int score { get; set; } // Pontuação do jogador
    [SerializeField] private TextMeshProUGUI scoreText;

    private TickTimer _shootCooldown;

    public override void Spawned()
    {
        UpdateScoreText();
        _rb = GetComponent<Rigidbody2D>();
        _shootCooldown = TickTimer.CreateFromSeconds(Runner, fireCooldown); // Initialize cooldown in Spawned
    }

    private void Update()
    {
        // Apenas o jogador com autoridade pode disparar
        if (HasInputAuthority && Input.GetKey(KeyCode.Space))
        {
            FireProjectile();
        }
    }

    private void FireProjectile()
    {
        // Verifica se o cooldown de disparo expirou
        if (_shootCooldown.ExpiredOrNotRunning(Runner))
        {
            FireProjectileRPC();
            _shootCooldown = TickTimer.CreateFromSeconds(Runner, fireCooldown);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void FireProjectileRPC()
    {
        // Verifica se o Runner pode spawnar objetos
        if (!Runner.CanSpawn) return;

        // Instancia o projétil na posição do firePoint
        NetworkObject projectile = Runner.Spawn(projectilePrefab, firePoint.position, firePoint.rotation, Object.InputAuthority);

        // Aplica a velocidade ao projétil
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = firePoint.up * projectileSpeed; // Move o projétil para frente
        }

        // Define o jogador como o dono do projétil para rastrear a pontuação
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetOwner(this);
        }
    }


    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // Normaliza a direção para evitar aceleração excessiva
            data.direction.Normalize();
            _rb.velocity = data.direction * moveSpeed;
        }
    }

    // Métodos para lidar com os Power-ups
    public void AddLives(int amount)
    {



        var playerData = GetComponent<PlayerDataNetworked>();
        var playerLives = playerData.Lives;
        if (playerLives >= 3)
        {


            if (playerData != null)
            {
                playerData.Lives += amount; // Adiciona vidas

            }

        }
        else
        {
            score += 100;
            UpdateScoreText();
        }
    }

    public void IncreaseFireRate(float amount, float duration)
    {
        fireCooldown = Mathf.Max(0.1f, fireCooldown - amount); // Aumenta a taxa de disparo, garantindo que não fique negativa
        StartCoroutine(FireRateBoostTimer(amount, duration));
        powerUpFireRate = true;
     
    }
    public void DecreaseFireRate(float amount)
    {
        fireCooldown = fireCooldownInitial;
        powerUpFireRate = false;
      
    }

    public void IncreaseSpeed(float amount, float duration)
    {
        moveSpeed += amount; // Aumenta a velocidade
        powerUpVelocity = true;

       

        // Inicia a corrotina para reverter a velocidade após o tempo do boost
        StartCoroutine(SpeedBoostTimer(amount, duration));
    }
    public void DecreaseSpeed(float amount)
    {
        moveSpeed -= amount;
        powerUpVelocity = false;
       
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcDestroyBullet(NetworkObject bullet)
    {
        if (bullet != null && Object.HasStateAuthority)
        {
            Runner.Despawn(bullet);
          
        }
    }
    private IEnumerator SpeedBoostTimer(float amount, float duration)
    {
        // Espera pelo tempo especificado (duração do boost)
        yield return new WaitForSeconds(duration);

        // Remove o boost de velocidade após o tempo definido
        DecreaseSpeed(amount);
       
    }

    private IEnumerator FireRateBoostTimer(float amount, float duration)
    {
        
        yield return new WaitForSeconds(duration);


        DecreaseFireRate(amount);
     
    }
    public void OnEnemyHit(int points)
    {
        UpdateScoreText(); // atualiza o score
        // Aumenta a pontuação do jogador
        score += points;
     
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }
}