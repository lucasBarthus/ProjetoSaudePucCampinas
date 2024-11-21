using Fusion;
using static Unity.Collections.Unicode;
using UnityEngine;

using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;


public class EnemySpawner : NetworkBehaviour
{ // Lista de prefabs de inimigos
    public List<NetworkObject> enemyPrefabs;
    public List<NetworkObject> powerUpPrefabs;
    public float spawnInterval = 3f; // Intervalo inicial de spawn
    public float spawnIntervalPowerEps;
    public float minX = -12f;
    public float maxX = 12f;
    public float minY = 20f;
    public float maxY = 25f;
    public GameObject boss;
    public GameObject boss2;
    public float BossAparece;
    private float timer;
    private float timerPowerUps;
    private float gameTime; // Contador de tempo de jogo
    public GameObject readyButton;
    private bool bossActivated = false;

    // Ajustes
    public int enemySpawnCount = 1; // Inimigos a serem spawnados inicialmente
    public float difficultyIncreaseInterval = 10f; // Intervalo para aumentar a dificuldade
    public float difficultyIncreaseFactor = 0.2f; // Fator de redução do intervalo de spawn
    public int maxEnemySpawnCount = 30; // Máximo de inimigos a serem spawnados

    public void Start()
    {
        spawnIntervalPowerEps = Random.Range(5f, 15f);
        StartCoroutine(ActivateBossAfterDelay(BossAparece));
        StartCoroutine(IncreaseDifficulty()); // Inicia a rotina de aumento de dificuldade
    }

    private void Update()
    {


        if (boss == null)
        {
            StartCoroutine(ActivateBossAfterDelay2(BossAparece));
        }

        if (Object.HasStateAuthority && gameObject.activeInHierarchy)
        {
            timer += Time.deltaTime;

            if (timer >= spawnInterval)
            {
                for (int i = 0; i < enemySpawnCount; i++)
                {
                    SpawnEnemy();
                }
                timer = 0f; // Reseta o contador de tempo
            }

            gameTime += Time.deltaTime; // Incrementa o tempo de jogo

            timerPowerUps += Time.deltaTime;

            if (timerPowerUps >= spawnIntervalPowerEps)
            {
                SpawnPowerUp();
                timerPowerUps = 0f; // Reseta o contador de tempo
                spawnIntervalPowerEps = Random.Range(10f, 15f);
            }
        }
    }

    private void SpawnPowerUp()
    {
        if (powerUpPrefabs != null && powerUpPrefabs.Count > 0)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                0
            );
            spawnPosition += transform.position;

            int randomIndex = Random.Range(0, powerUpPrefabs.Count);
            NetworkObject chosenPowerUpPrefab = powerUpPrefabs[randomIndex];

            NetworkObject powerUpObject = Runner.Spawn(chosenPowerUpPrefab, spawnPosition, Quaternion.identity, Object.InputAuthority);
        }
        else
        {
            Debug.LogWarning("Nenhum prefab de Power-up foi atribuído na lista.");
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs != null && enemyPrefabs.Count > 0)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                0
            );
            spawnPosition += transform.position;

            int randomIndex = Random.Range(0, enemyPrefabs.Count);
            NetworkObject chosenEnemyPrefab = enemyPrefabs[randomIndex];

            NetworkObject enemyObject = Runner.Spawn(chosenEnemyPrefab, spawnPosition, Quaternion.identity, Object.InputAuthority);
        }
        else
        {
            Debug.LogWarning("Nenhum prefab de inimigo foi atribuído na lista.");
        }
    }

    private IEnumerator IncreaseDifficulty()
    {
        while (true)
        {
            yield return new WaitForSeconds(difficultyIncreaseInterval);

            // Aumenta a quantidade de inimigos spawnados
            if (enemySpawnCount < maxEnemySpawnCount)
            {
                enemySpawnCount++;
            }

            // Diminui o intervalo de spawn
            spawnInterval *= difficultyIncreaseFactor;

            // Log para debugar
            Debug.Log($"Dificuldade aumentada! Tempo de spawn: {spawnInterval}, Inimigos por spawn: {enemySpawnCount}");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcDestroyEnemy(NetworkObject enemy)
    {
        if (enemy != null)
        {
            Runner.Despawn(enemy);
            Debug.Log($"Inimigo destruído: {enemy}");
        }
    }

    public void ActivateSpawner()
    {
        gameObject.SetActive(true);
        Debug.Log("EnemySpawner ativado.");
        readyButton.SetActive(false); // Desativa o botão
    }

    private IEnumerator ActivateBossAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (boss != null && !bossActivated)
        {
            boss.SetActive(true);
            bossActivated = true;
            Debug.Log("Boss ativado!");
        }
        else
        {
            Debug.LogWarning("O Boss não foi atribuído ou já foi ativado.");
        }
    }
    private IEnumerator ActivateBossAfterDelay2(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (boss2 != null)
        {
            boss2.SetActive(true);
          
            Debug.Log("Boss ativado!");
        }
        else
        {
            Debug.LogWarning("O Boss não foi atribuído ou já foi ativado.");
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position + new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 1);
        Gizmos.DrawWireCube(center, size);
    }
}