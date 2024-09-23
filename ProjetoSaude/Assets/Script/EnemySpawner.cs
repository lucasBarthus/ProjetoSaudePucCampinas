using Fusion;
using static Unity.Collections.Unicode;
using UnityEngine;

using System.Collections;
using UnityEngine.UI;

public class EnemySpawner : NetworkBehaviour
{
    public NetworkObject enemyPrefab;
    public float spawnInterval; // Intervalo de tempo entre cada spawn
    public float minX = -12f; // Limite mínimo no eixo X para a posição do spawn
    public float maxX = 12f; // Limite máximo no eixo X para a posição do spawn
    public float minY = 20f; // Limite mínimo no eixo Y para a posição do spawn
    public float maxY = 25f; // Limite máximo no eixo Y para a posição do spawn

    private float timer; // Contador de tempo para controlar os spawns
    public GameObject readyButton;

    private void Update()
    {
        if (Object.HasStateAuthority && gameObject.activeInHierarchy)
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
        // Gera uma posição aleatória dentro dos limites definidos
        Vector3 spawnPosition = new Vector3(
            UnityEngine.Random.Range(minX, maxX),
            UnityEngine.Random.Range(minY, maxY),
            0
        );

        // Ajusta a posição com a posição do spawner
        spawnPosition += transform.position;

        // Spawna o inimigo usando o `Runner`
        if (enemyPrefab != null)
        {
            NetworkObject enemyObject = Runner.Spawn(enemyPrefab, spawnPosition, Quaternion.identity, Object.InputAuthority);
            spawnInterval = UnityEngine.Random.Range(0.1f, 1f); // Varia o intervalo entre spawns
        }
        else
        {
            Debug.LogWarning("O prefab do inimigo não está definido.");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcDestroyEnemy(NetworkObject enemy)
    {
        if (enemy != null)
        {
            Runner.Despawn(enemy); // Despawns o inimigo
            Debug.Log($"Inimigo destruído: {enemy}");
        }
    }

    public void ActivateSpawner()
    {
        gameObject.SetActive(true);
        Debug.Log("EnemySpawner ativado.");
        readyButton.SetActive(false); // Desativa o botão
    }

    // Método para desenhar gizmos no editor
    private void OnDrawGizmos()
    {
        // Definindo a cor dos gizmos
        Gizmos.color = Color.red;

        // Desenhando uma caixa que representa a área de spawn
        Vector3 center = transform.position + new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 1);
        Gizmos.DrawWireCube(center, size);
    }
}