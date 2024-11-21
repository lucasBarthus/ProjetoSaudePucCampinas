using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpColor : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float changeInterval = 1.0f; // Intervalo em segundos entre as mudanças de cor
    private float timer;

    // Cores saturadas que serão usadas
    private Color[] saturatedColors = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        new Color(1.0f, 0.0f, 1.0f), // Magenta
        new Color(1.0f, 0.5f, 0.0f), // Laranja
        new Color(0.0f, 1.0f, 1.0f), // Ciano
    };

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ChangeColor(); // Inicia com uma cor aleatória
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= changeInterval)
        {
            timer = 0f;
            ChangeColor();
        }
    }

    void ChangeColor()
    {
        // Seleciona uma cor aleatória da lista
        int randomIndex = Random.Range(0, saturatedColors.Length);
        spriteRenderer.color = saturatedColors[randomIndex];
    }
}
