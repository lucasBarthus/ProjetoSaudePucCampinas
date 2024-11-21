using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    public GameObject projectilePrefab; // Prefab do projétil
    public float shootInterval = 2.0f;  // Intervalo entre tiros
    public float projectileSpeed = 5.0f; // Velocidade do projétil

    private void Start()
    {
        // Inicia o disparo contínuo
        StartCoroutine(ShootProjectile());
    }

    private IEnumerator ShootProjectile()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootInterval); // Espera o intervalo antes de disparar

            // Decide aleatoriamente a direção (1 para direita, -1 para esquerda)
            int direction = Random.value > 0.5f ? 1 : -1;

            // Instancia o projétil na posição do inimigo
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            // Define a velocidade e a direção do projétil
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = new Vector2(direction * projectileSpeed, 0);
            }

            // Destroi o projétil após alguns segundos para não ocupar memória
            Destroy(projectile, 5.0f);
        }
    }
}
