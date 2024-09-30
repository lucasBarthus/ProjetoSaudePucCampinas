using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : NetworkBehaviour
{
    public float moveSpeed = 2f;
    public Transform middlePoint;
    public Animator animator;
    public float shootInterval = 3f;
    public NetworkPrefabRef projectilePrefab;
    public Transform shootPoint;
    public float projectileSpeed = 5f;
    public float projectileLifetime = 5f;
    public float horizontalMoveDistance = 5f;

    [SerializeField] private int vida = 5; // Vida inicial do boss
    private Vector3 leftLimit;
    private Vector3 rightLimit;
    private Vector3 targetPosition;
    private bool isAtMiddle = false;
    private bool movingRight = true;
    private float shootTimer = 0f;

    private SpriteRenderer spriteRenderer; // Adicione esta linha
    public Color hitColor = Color.red; // Cor para quando o boss for atingido
    public float flashDuration = 0.1f; // Duração do flash

    public void Start()
    {
        targetPosition = middlePoint.position;
        leftLimit = new Vector3(middlePoint.position.x - horizontalMoveDistance, middlePoint.position.y, middlePoint.position.z);
        rightLimit = new Vector3(middlePoint.position.x + horizontalMoveDistance, middlePoint.position.y, middlePoint.position.z);
        spriteRenderer = GetComponent<SpriteRenderer>(); // Atribua o SpriteRenderer
    }

    void Update()
    {
        if (!isAtMiddle)
        {
            MoveToMiddle();
        }
        else
        {
            HorizontalMovement();
            shootTimer += Time.deltaTime;

            if (shootTimer >= shootInterval)
            {
                Shoot();
                shootTimer = 0f;
            }
        }
    }

    private void MoveToMiddle()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            isAtMiddle = true;
            animator.SetBool("IsShooting", true);
        }
    }

    private void HorizontalMovement()
    {
        if (movingRight)
        {
            transform.position = Vector2.MoveTowards(transform.position, rightLimit, moveSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, rightLimit) < 0.1f)
            {
                movingRight = false;
            }
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, leftLimit, moveSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, leftLimit) < 0.1f)
            {
                movingRight = true;
            }
        }
    }

    private void Shoot()
    {
        animator.SetTrigger("Shoot");
    }

    public void BossShoot()
    {
        var projectile = Runner.Spawn(projectilePrefab, shootPoint.position, shootPoint.rotation, Object.InputAuthority);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = shootPoint.right * projectileSpeed;
        }

        StartCoroutine(DespawnProjectile(projectile, projectileLifetime));
    }

    private IEnumerator DespawnProjectile(NetworkObject projectile, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        if (projectile != null && projectile.IsValid)
        {
            Runner.Despawn(projectile);
        }
    }

    // Método para receber dano e verificar se o boss morreu
    public void TakeDamage(int amount)
    {
        vida -= amount;
        Debug.Log($"O boss foi atingido! Vida restante: {vida}");

        // Feedback visual para o dano
        StartCoroutine(FlashRed());

        // Se a vida do boss for menor ou igual a zero
        if (vida <= 0)
        {
            OnBossDefeated();
        }
    }

    // Método chamado quando o boss é derrotado
    private void OnBossDefeated()
    {
        Debug.Log("O boss foi derrotado!");
        // Aqui você pode adicionar lógica para pausar o jogo e exibir a tela de vitória
    }

    // Corrotina para o feedback visual de piscar vermelho
    private IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            // Salva a cor original do boss
            Color originalColor = spriteRenderer.color;

            // Muda a cor do boss para vermelho
            spriteRenderer.color = hitColor;

            // Espera pela duração do flash
            yield return new WaitForSeconds(flashDuration);

            // Restaura a cor original
            spriteRenderer.color = originalColor;
        }
    }
}