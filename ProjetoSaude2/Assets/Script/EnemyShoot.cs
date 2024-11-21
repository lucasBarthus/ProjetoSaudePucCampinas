using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    public GameObject projectilePrefab; // Prefab do proj�til
    public float shootInterval = 2.0f;  // Intervalo entre tiros
    public float projectileSpeed = 5.0f; // Velocidade do proj�til

    private void Start()
    {
        // Inicia o disparo cont�nuo
        StartCoroutine(ShootProjectile());
    }

    private IEnumerator ShootProjectile()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootInterval); // Espera o intervalo antes de disparar

            // Decide aleatoriamente a dire��o (1 para direita, -1 para esquerda)
            int direction = Random.value > 0.5f ? 1 : -1;

            // Instancia o proj�til na posi��o do inimigo
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            // Define a velocidade e a dire��o do proj�til
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = new Vector2(direction * projectileSpeed, 0);
            }

            // Destroi o proj�til ap�s alguns segundos para n�o ocupar mem�ria
            Destroy(projectile, 5.0f);
        }
    }
}
