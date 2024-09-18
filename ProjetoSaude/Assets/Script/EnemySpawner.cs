using Fusion;
using static Unity.Collections.Unicode;
using UnityEngine;

using System.Collections;
using UnityEngine.UI;


public class EnemySpawner : NetworkBehaviour
{

    public NetworkObject enemyPrefab;
    public NetworkObjectPoolDefault objectPool; // Referência à pool de objetos
    public float spawnInterval; // Intervalo de tempo entre cada spawn
    public float minX = -12f; // Limite mínimo no eixo X para a posição do spawn
    public float maxX = 12f; // Limite máximo no eixo X para a posição do spawn
    public float minY = 20f; // Limite mínimo no eixo Y para a posição do spawn
    public float maxY = 25f; // Limite máximo no eixo Y para a posição do spawn

    private float timer; // Contador de tempo para controlar os spawns
    public GameObject readyButton;
    private void Start()
    {
        gameObject.SetActive(false); // Desativa o spawner inicialmente
        
    }

    private void Update()
    {
        if (Object.HasStateAuthority)
        {
            if (!gameObject.activeInHierarchy)
            {
                return; // Não faz nada se o spawner estiver desativado
            }

            timer += Time.deltaTime;

            if (timer >= spawnInterval)
            {
                SpawnEnemy();
                timer = 0f; // Reseta o contador de tempo
            }
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY), 0);
        spawnPosition += transform.position;

        // Obtenha o inimigo da pool
        NetworkObject enemyObject = objectPool.GetObjectFromPool(enemyPrefab);

        // Verifica se conseguiu obter o objeto da pool antes de fazer o spawn
        if (enemyObject != null)
        {
            // Defina a posição e rotação do inimigo antes de spawná-lo na rede
            enemyObject.transform.position = spawnPosition;
            enemyObject.transform.rotation = Quaternion.identity;

            // Faça o spawn do inimigo na rede apenas pelo Host
            Runner.Spawn(enemyObject, spawnPosition, Quaternion.identity, Object.InputAuthority);

            // Atualize o intervalo de spawn
            spawnInterval = UnityEngine.Random.Range(0.1f, 1f);
        }
        else
        {
            Debug.LogWarning("Não foi possível obter um objeto da pool.");
        }
    }
    public void ActivateSpawner()
    {
        // Ative o spawner aqui. 
        // Por exemplo, você pode ativar um GameObject ou habilitar um componente.
        gameObject.SetActive(true); // Exemplo de ativação
        Debug.Log("EnemySpawner ativado.");
    }

}