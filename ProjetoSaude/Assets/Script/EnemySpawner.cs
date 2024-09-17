using Fusion;
using static Unity.Collections.Unicode;
using UnityEngine;

using System.Collections;


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

    private void Start()
    {
        StartCoroutine(StartSpawningWithDelay(5f)); // Inicia a rotina com um atraso de 5 segundos
    }

    private IEnumerator StartSpawningWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Aguarda o tempo especificado
        spawnInterval = UnityEngine.Random.Range(1f, 5f); // Define o intervalo de spawn após o atraso
    }

    private void Update()
    {
        if (Runner.IsServer)
        {
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

        // Defina a posição e rotação do inimigo antes de spawná-lo na rede
        enemyObject.transform.position = spawnPosition;
        enemyObject.transform.rotation = Quaternion.identity;

        // Faça o spawn do inimigo na rede
        Runner.Spawn(enemyObject, spawnPosition, Quaternion.identity, Object.InputAuthority);

        // Atualize o intervalo de spawn
        spawnInterval = UnityEngine.Random.Range(0.1f, 1f);
    }
}