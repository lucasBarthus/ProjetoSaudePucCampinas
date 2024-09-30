using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollision : NetworkBehaviour
{
    [SerializeField] private int vida = 3; // Vida inicial do inimigo
    public float speed = 50f;
    public Color hitColor = Color.red; // Cor para quando o inimigo for atingido
    public float flashDuration = 0.1f; // Duração do flash vermelho
    private SpriteRenderer spriteRenderer; // Referência ao SpriteRenderer para mudar a cor
    private EnemySpawner spawner;
    public NetworkObject NetworkObject { get; private set; }


  
    public void Start()
    {
        spawner = FindAnyObjectByType<EnemySpawner>();
    
        NetworkObject = GetComponent<NetworkObject>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public override void FixedUpdateNetwork()
    {
        MoveDown();
    }

    private void MoveDown()
    {
        // Move o inimigo para baixo usando Translate
        transform.Translate(Vector3.down * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            if (Object.HasStateAuthority)
            {
                // Reduz a vida ao ser atingido por uma bala
                RPC_EnemyHit("Bullet");
            }
        }
        if (collision.CompareTag("Wall"))
        {
          
            spawner.RpcDestroyEnemy(NetworkObject);
        }
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_EnemyHit(string source)
    {
        vida--; // Reduz a vida do inimigo
        Debug.Log($"O inimigo foi atingido por {source}! Vida restante: {vida}");

        // Feedback visual para o dano
        StartCoroutine(FlashRed());

        if (vida < 1)
        {
            if (NetworkObject.HasStateAuthority)
            {
               
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

    // Corrotina para o feedback visual de piscar vermelho
    private IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            // Salva a cor original do inimigo
            Color originalColor = spriteRenderer.color;

            // Muda a cor do inimigo para vermelho
            spriteRenderer.color = hitColor;

            // Espera pela duração do flash
            yield return new WaitForSeconds(flashDuration);

            // Restaura a cor original
            spriteRenderer.color = originalColor;
        }
    }
}