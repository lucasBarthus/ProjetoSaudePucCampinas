using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollision : NetworkBehaviour
{
    public enum EnemyType
    {
        Fall,     // Tipo que s� cai
        Chase     // Tipo que persegue o jogador
    }
    [SerializeField] private int pointsForKill = 50;
    [SerializeField] public int vida = 3; // Vida inicial do inimigo
    public float speed = 50f;
    public Color hitColor = Color.red; // Cor para quando o inimigo for atingido
    public float flashDuration = 0.1f; // Dura��o do flash vermelho
    private SpriteRenderer spriteRenderer; // Refer�ncia ao SpriteRenderer para mudar a cor
    private EnemySpawner spawner;
    public Color originalColor;
    public NetworkObject NetworkObject { get; private set; }

    [SerializeField] private NetworkPrefabRef explosionPrefab; // Refer�ncia ao prefab da explos�o
    [SerializeField] private Color explosionColor; // cor da explos�o 

    public EnemyType enemyType = EnemyType.Fall; // Escolhe o tipo do inimigo no Inspector

    public Transform targetPlayer; // O jogador alvo que o inimigo vai perseguir (caso seja do tipo Chase)

    public void Start()
    {

        
        spawner = FindAnyObjectByType<EnemySpawner>();

        NetworkObject = GetComponent<NetworkObject>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        // Se o tipo for "Chase", tenta encontrar o jogador mais pr�ximo
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
        // Calcula a dire��o do jogador
        Vector3 direction = (targetPlayer.position - transform.position).normalized;

        // Move o inimigo na dire��o do jogador
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Faz o inimigo "olhar" para o jogador, ajustando a rota��o no eixo Z
        LookAtPlayer(direction);
    }

    private void LookAtPlayer(Vector3 direction)
    {
        // Calcula o �ngulo em graus baseado na dire��o
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Aplica a rota��o no eixo Z para o inimigo "olhar" para o jogador
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f)); // -90f � para ajustar a rota��o correta
    }

    private void FindClosestPlayer()
    {
        // Encontra todos os objetos com a tag "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDistance = Mathf.Infinity;
        GameObject closestPlayer = null;

        foreach (GameObject player in players)
        {
            // Calcula a dist�ncia entre o inimigo e cada jogador
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        if (closestPlayer != null)
        {
            // Define o jogador mais pr�ximo como o alvo
            targetPlayer = closestPlayer.transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            if (Object.HasStateAuthority)
            {
                // Pega o componente do proj�til para acessar o jogador que disparou
                Projectile projectile = collision.GetComponent<Projectile>();
                if (projectile != null)
                {
                    // Passa a refer�ncia do jogador que disparou o proj�til
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

                // Instancia a explos�o na rede
                SpawnExplosion();

                if (spawner != null)
                {
                    spawner.RpcDestroyEnemy(NetworkObject); // Chama o RPC para destruir o inimigo
                }
                else
                {
                    Debug.LogWarning("EnemySpawner n�o encontrado.");
                }
            }

            Debug.Log($"O inimigo foi destru�do!");
        }
    }

    // M�todo para instanciar a explos�o na rede
    private void SpawnExplosion()
    {
        if (explosionPrefab != null)
        {
            // Spawna o prefab de explos�o na posi��o do inimigo
            NetworkObject explosion = Runner.Spawn(explosionPrefab, transform.position, Quaternion.identity);

            // Atribui a cor espec�fica para a explos�o
            SpriteRenderer explosionSpriteRenderer = explosion.GetComponent<SpriteRenderer>();
            if (explosionSpriteRenderer != null)
            {
                explosionSpriteRenderer.color = explosionColor;
            }

            // Inicia a corrotina para destruir a explos�o ap�s um tempo
            StartCoroutine(DestroyExplosionAfterTime(explosion, 2f)); // 2 segundos de tempo antes de destruir
        }
        else
        {
            Debug.LogWarning("Prefab de explos�o n�o atribu�do.");
        }
    }

    // Corrotina para destruir a explos�o ap�s um curto per�odo de tempo
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

            // Espera pela dura��o do flash
            yield return new WaitForSeconds(flashDuration);

            // Restaura a cor original
            spriteRenderer.color = originalColor;
        }
    }
}  