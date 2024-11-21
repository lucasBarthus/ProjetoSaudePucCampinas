using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


// Define a estrutura de entrada para o movimento 2D
public struct NetworkInputData : INetworkInput
{
    public Vector2 direction; // Usa Vector2 para movimento em 2D
    public bool fire;
}
