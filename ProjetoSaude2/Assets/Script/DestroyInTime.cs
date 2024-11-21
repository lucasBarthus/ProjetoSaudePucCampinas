using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyInTime : MonoBehaviour
{
      [SerializeField] private float destroyDelay = 2f; // Tempo at� a destrui��o

    // Start � chamado antes do primeiro frame update
    void Start()
    {
        // Chama a fun��o para destruir o objeto ap�s o tempo especificado
        Destroy(gameObject, destroyDelay);
    }
}
