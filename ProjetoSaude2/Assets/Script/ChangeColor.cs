using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : NetworkBehaviour
{
    private SpriteRenderer spriteRenderer;

    // Vari�vel de cor que ser� sincronizada entre todos os jogadores
    [Networked]
    public Vector3 PlayerColor { get; set; }

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Atualiza a cor inicial com base no valor sincronizado
        UpdatePlayerColor(PlayerColor);
    }

    private void Update()
    {
        // Verifica se o jogador tem autoridade sobre o estado (StateAuthority) e se a tecla 'C' foi pressionada
        if (HasStateAuthority && Input.GetKeyDown(KeyCode.C))
        {
            // Gera uma cor aleat�ria
            Color randomColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            // Altera a cor e sincroniza com todos os jogadores via vari�vel de rede
            ChangeColor2(randomColor);
        }
    }

    // M�todo que muda a cor localmente e define a vari�vel sincronizada
    void ChangeColor2(Color newColor)
    {
        // Sincroniza a cor para todos os jogadores via vari�vel de rede
        PlayerColor = new Vector3(newColor.r, newColor.g, newColor.b);
    }

    // Atualiza a cor do SpriteRenderer com base na cor sincronizada a cada frame
    private void FixedUpdate()
    {
        // Atualiza a cor localmente para todos os jogadores com base na vari�vel sincronizada
        UpdatePlayerColor(PlayerColor);
    }

    // M�todo que atualiza a cor do SpriteRenderer
    void UpdatePlayerColor(Vector3 newColor)
    {
        // Converte o Vector3 para Color e aplica ao SpriteRenderer
        spriteRenderer.color = new Color(newColor.x, newColor.y, newColor.z);
    }
}