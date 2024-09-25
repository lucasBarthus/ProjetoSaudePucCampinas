using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : NetworkBehaviour
{
    public float moveSpeed = 2f; // Velocidade do boss
    public Transform middlePoint; // Ponto de destino (meio da tela)
    public Animator animator; // Referência ao Animator
    public float shootInterval = 3f; // Intervalo entre os disparos

    private Vector3 targetPosition;
    private bool isAtMiddle = false;
    private float shootTimer = 0f;

    // Método chamado quando o objeto de rede é spawnado
    public void Start()
    {
        targetPosition = middlePoint.position; // Definir o destino para o meio da tela
    }

    void Update()
    {
       // if (!Object.HasStateAuthority) return; // Garantir que somente o dono do objeto controla a lógica

        if (!isAtMiddle)
        {
            MoveToMiddle();
        }
        else
        {
            shootTimer += Time.deltaTime;

            if (shootTimer >= shootInterval)
            {
                Shoot();
                shootTimer = 0f; // Reseta o timer
            }
        }
    }

    private void MoveToMiddle()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            isAtMiddle = true;
            animator.SetBool("IsShooting", true); // Ativa a animação de atirar
        }
    }

    private void Shoot()
    {
        animator.SetTrigger("Shoot"); // Aciona a animação de disparo
    }

    // Este método será chamado através do Animation Event 'BossShoot'
    public void BossShoot()
    {
        // Lógica para instanciar o projétil (implementado futuramente)
        Debug.Log("Boss disparou!");
    }
}

