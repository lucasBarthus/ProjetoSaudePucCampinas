using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyInTime : MonoBehaviour
{
      [SerializeField] private float destroyDelay = 2f; // Tempo até a destruição

    // Start é chamado antes do primeiro frame update
    void Start()
    {
        // Chama a função para destruir o objeto após o tempo especificado
        Destroy(gameObject, destroyDelay);
    }
}
