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
    public GameObject bossPrefab;
    public float BossApareceDelay = 3f;
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    private Vector3 leftLimit;
    private Vector3 rightLimit;
    private Vector3 targetPosition;
    private bool isAtMiddle = false;
    private bool movingRight = true;
    private float shootTimer = 0f;
    private bool bossDefeated = false;
    private bool bossActivated = false;

    private SpriteRenderer spriteRenderer;
    private EnemyCollision enemyCollision; // Referência ao script EnemyCollision

    void Start()
    {
        targetPosition = middlePoint.position;
        leftLimit = new Vector3(middlePoint.position.x - horizontalMoveDistance, middlePoint.position.y, middlePoint.position.z);
        rightLimit = new Vector3(middlePoint.position.x + horizontalMoveDistance, middlePoint.position.y, middlePoint.position.z);
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Obtém o componente EnemyCollision
        enemyCollision = GetComponent<EnemyCollision>();
    }

    void Update()
    {
        if (!isAtMiddle)
        {
            MoveToMiddle();
        }
        else if (!bossDefeated) // Verifica a vida do boss enquanto ele não foi derrotado
        {
            HorizontalMovement();
            shootTimer += Time.deltaTime;

            if (shootTimer >= shootInterval)
            {
                Shoot();
                shootTimer = 0f;
            }

            // Checa se a vida do boss chegou a zero
            if (enemyCollision != null && enemyCollision.vida <= 0)
            {
                OnBossDefeated();
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

    private void OnBossDefeated()
    {
        bossDefeated = true;
        animator.SetBool("IsShooting", false);
        StartCoroutine(ActivateNextBossAfterDelay(BossApareceDelay));
        Debug.Log("O boss foi derrotado!");
    }

    private IEnumerator ActivateNextBossAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!bossActivated)
        {
            Instantiate(bossPrefab, middlePoint.position, Quaternion.identity);
            bossActivated = true;
            Debug.Log("Novo boss ativado!");
        }
        else
        {
            Debug.LogWarning("O próximo boss já foi ativado ou atribuído incorretamente.");
        }
    }
}
