using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : NetworkBehaviour
{

    public float scaleSpeed = 0.5f; // Velocidade de crescimento
    public Vector3 maxScale = new Vector3(3f, 3f, 3f); // Escala máxima

    // O Update é chamado em todos os frames
    void Update()
    {
        // Se o objeto for controlado pela rede (somente o owner ou o servidor)
        if (Object.HasStateAuthority)
        {
            // Chama a função de aumentar o scale
            ScaleOverTime();
        }
    }

    [Networked] // Sincroniza o scale na rede
    public Vector3 CurrentScale { get; set; } = Vector3.one;

    private void ScaleOverTime()
    {
        // Aumenta o scale do projetil se ele não tiver atingido o máximo
        if (CurrentScale.x < maxScale.x || CurrentScale.y < maxScale.y || CurrentScale.z < maxScale.z)
        {
            CurrentScale += Vector3.one * scaleSpeed * Time.deltaTime;

            // Limita o scale ao valor máximo definido
            CurrentScale = new Vector3(
                Mathf.Min(CurrentScale.x, maxScale.x),
                Mathf.Min(CurrentScale.y, maxScale.y),
                Mathf.Min(CurrentScale.z, maxScale.z)
            );

            // Aplica o novo scale ao transform local
            transform.localScale = CurrentScale;
        }
    }

    // Chama esse método quando o projetil for spawnado para sincronizar o scale inicial
    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            CurrentScale = transform.localScale; // Sincroniza a escala inicial
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Aplicar a escala sincronizada em todos os clientes
        transform.localScale = CurrentScale;
    }
}