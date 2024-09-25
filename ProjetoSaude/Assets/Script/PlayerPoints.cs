using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class PlayerPoints : MonoBehaviour
{
    public TextMeshProUGUI PlayerPointText;
    private PlayerMovementFusion player;
    private int playerPoints; // Variável para armazenar a pontuação do jogador

    private void Start()
    {
        PlayerPointText = GameObject.Find("PointsText").GetComponent<TextMeshProUGUI>();
        playerPoints = 0; // Inicializa a pontuação do jogador
        UpdatePointsText(); // Atualiza o texto no início
    }

    // Método para adicionar pontos
    public void AddPoints(int points)
    {
        playerPoints += points; // Adiciona os pontos
        UpdatePointsText(); // Atualiza o texto
    }

    // Método para atualizar o texto
    private void UpdatePointsText()
    {
        PlayerPointText.text = "Pontos: " + playerPoints; // Atualiza o texto com a pontuação
    }

    // (Opcional) Se você quiser definir o jogador para receber pontos
    public void SetPlayer(PlayerMovementFusion newPlayer)
    {
        player = newPlayer;
    }
}