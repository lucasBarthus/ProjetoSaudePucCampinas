using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollision : NetworkBehaviour
{
    public enum EnemyType
    {
        Fall,     // Tipo que só cai
        Chase     // Tipo que persegue o jogador
    }
    [SerializeField] private int pointsForKill = 50;
    [SerializeField] private int vida = 3; // Vida inicial do inimigo
    public float speed = 50f;
    public Color hitColor = Color.red; // Cor para quando o inimigo for atingido
    public float flashDuration = 0.1f; // Duração do flash vermelho
    private SpriteRenderer spriteRenderer; // Referência ao SpriteRenderer para mudar a cor
    private EnemySpawner spawner;
    public Color originalColor;
    public NetworkObject NetworkObject { get; private set; }

    [SerializeField] private NetworkPrefabRef explosionPrefab; // Referência ao prefab da explosão
    [SerializeField] private Color explosionColor; // cor da explosão 

    public EnemyType enemyType = EnemyType.Fall; // Escolhe o tipo do inimigo no Inspector

    public Transform targetPlayer; // O jogador alvo que o inimigo vai perseguir (caso seja do tipo Chase)

    public void Start()
    {

        
        spawner = FindAnyObjectByType<EnemySpawner>();

        NetworkObject = GetComponent<NetworkObject>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        // Se o tipo for "Chase", tenta encontrar o jogador mais próximo
        if (enemyType == EnemyType.Chase)
        {
            FindClosestPlayer();
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Verifica o tipo de inimigo e aplica o comportamento adequado
        if (enemyType == EnemyType.Fall)
        {
            MoveDown();
        }
        else if (enemyType == EnemyType.Chase && targetPlayer != null)
        {
            ChasePlayer();
        }
    }

    private void MoveDown()
    {
        // Move o inimigo para baixo usando Translate
        transform.Translate(Vector3.down * speed * Time.deltaTime);
    }

    private void ChasePlayer()
    {
        // Calcula a direção do jogador
        Vector3 direction = (targetPlayer.position - transform.position).normalized;

        // Move o inimigo na direção do jogador
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Faz o inimigo "olhar" para o jogador, ajustando a rotação no eixo Z
        LookAtPlayer(direction);
    }

    private void LookAtPlayer(Vector3 direction)
    {
        // Calcula o ângulo em graus baseado na direção
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Aplica a rotação no eixo Z para o inimigo "olhar" para o jogador
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f)); // -90f é para ajustar a rotação correta
    }

    private void FindClosestPlayer()
    {
        // Encontra todos os objetos com a tag "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDistance = Mathf.Infinity;
        GameObject closestPlayer = null;

        foreach (GameObject player in players)
        {
            // Calcula a distância entre o inimigo e cada jogador
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        if (closestPlayer != null)
        {
            // Define o jogador mais próximo como o alvo
            targetPlayer = closestPlayer.transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            if (Object.HasStateAuthority)
            {
                // Pega o componente do projétil para acessar o jogador que disparou
                Projectile projectile = collision.GetComponent<Projectile>();
                if (projectile != null)
                {
                    // Passa a referência do jogador que disparou o projétil
                    PlayerMovementFusion player = projectile.GetOwner();
                    if (player != null)
                    {
                        RPC_EnemyHit("Bullet", player);
                    }
                }
            }
        }
        if (collision.CompareTag("Wall"))
        {
            spawner.RpcDestroyEnemy(NetworkObject);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_EnemyHit(string source, PlayerMovementFusion player)
    {
        vida--; // Reduz a vida do inimigo

        // Feedback visual para o dano
        StartCoroutine(FlashRed());

        if (vida < 1)
        {
            if (NetworkObject.HasStateAuthority)
            {
                // Concede pontos ao jogador
                if (player != null)
                {
                    player.OnEnemyHit(pointsForKill); // Adiciona os pontos com base no tipo do inimigo
                }

                // Instancia a explosão na rede
                SpawnExplosion();

                if (spawner != null)
                {
                    spawner.RpcDestroyEnemy(NetworkObject); // Chama o RPC para destruir o inimigo
                }
                else
                {
                    Debug.LogWarning("EnemySpawner não encontrado.");
                }
            }

            Debug.Log($"O inimigo foi destruído!");
        }
    }

    // Método para instanciar a explosão na rede
    private void SpawnExplosion()
    {
        if (explosionPrefab != null)
        {
            // Spawna o prefab de explosão na posição do inimigo
            NetworkObject explosion = Runner.Spawn(explosionPrefab, transform.position, Quaternion.identity);

            // Atribui a cor específica para a explosão
            SpriteRenderer explosionSpriteRenderer = explosion.GetComponent<SpriteRenderer>();
            if (explosionSpriteRenderer != null)
            {
                explosionSpriteRenderer.color = explosionColor;
            }

            // Inicia a corrotina para destruir a explosão após um tempo
            StartCoroutine(DestroyExplosionAfterTime(explosion, 2f)); // 2 segundos de tempo antes de destruir
        }
        else
        {
            Debug.LogWarning("Prefab de explosão não atribuído.");
        }
    }

    // Corrotina para destruir a explosão após um curto período de tempo
    private IEnumerator DestroyExplosionAfterTime(NetworkObject explosion, float delay)
    {
        // Espera pelo tempo especificado (delay)
        yield return new WaitForSeconds(delay);

        // Despawns the explosion network object
        if (explosion != null && explosion.HasStateAuthority)
        {
            Runner.Despawn(explosion);
        }
    }

    // Corrotina para o feedback visual de piscar vermelho
    private IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            // Muda a cor do inimigo para vermelho
            spriteRenderer.color = hitColor;

            // Espera pela duração do flash
            yield return new WaitForSeconds(flashDuration);

            // Restaura a cor original
            spriteRenderer.color = originalColor;
        }
    }
}  