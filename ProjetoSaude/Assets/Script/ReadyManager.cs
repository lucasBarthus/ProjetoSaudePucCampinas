using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;


public class ReadyManager : NetworkBehaviour
{
    private NetworkRunner runner;
    public Button readyButton;
    public bool gameIsReady = false;
    public GameObject gameOverScreen;
    public static ReadyManager Instance { get; private set; }
    int deadCount = 0;

    [SerializeField] private TextMeshProUGUI playerListText;

    [SerializeField] public EnemySpawner enemySpawner;

    private void Start()
    {
        Instance = this;

        runner = FusionManager.runnerInstance;
        if (runner == null)
        {
            Debug.LogError("NetworkRunner não está inicializado.");
            return;
        }

        // Configura o botão para chamar o OnClickReadyButton quando clicado
        readyButton.onClick.AddListener(OnClickReadyButton);
    }

    private void Update()
    {
        // Se ambos os jogadores estiverem mortos, exiba a tela de Game Over
        if (deadCount >= 2)
        {
            ShowGameOverScreen();
        }

        if (runner != null)
        {
            //tirar essas duas funções do update futuramente 
            CheckPlayerCount();
            UpdatePlayerList();
        }
    }

    private void CheckPlayerCount()
    {
        var activePlayers = runner.ActivePlayers;

        int playerCount = 0;

        if (activePlayers is ICollection<PlayerRef> collection)
        {
            playerCount = collection.Count;
        }
        else if (activePlayers is IEnumerable<PlayerRef> enumerable)
        {
            playerCount = enumerable.Count();
        }
        else
        {
            Debug.LogError("Tipo de ActivePlayers não suportado para contagem.");
        }

        // Exibe o botão "Ready" se houver pelo menos 2 jogadores e o jogo não estiver pronto
        readyButton.gameObject.SetActive(playerCount >= 2 && !gameIsReady);
    }
    public void OnClickReadyButton()
    {
        Debug.Log("Botão Ready clicado");
        RPC_GameReady();
    }

    // Função RPC para sincronizar o estado entre os jogadores
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_GameReady()
    {
        if (!gameIsReady)
        {
            Debug.Log("o jogo começou");
            gameIsReady = true;
            enemySpawner.enabled = true;
            readyButton.gameObject.SetActive(false);  // Oculta o botão para todos
            runner.SessionInfo.IsOpen = false;
            Debug.Log("Sala fechada. Ninguém mais pode entrar.");
            
        }
    }
    private void UpdatePlayerList()
    {
        var activePlayers = runner.ActivePlayers;

        // Cria uma lista de nomes de jogadores
        List<string> playerNames = new List<string>();

        foreach (var player in activePlayers)
        {
            playerNames.Add(player.ToString());
        }

        // Atualiza o texto com a lista de jogadores
        playerListText.text = "Jogadores:\n" + string.Join("\n", playerNames);
    }

   public void CheckPlayersDead()
    {
        deadCount++;
    }

    private void ShowGameOverScreen()
    {
        Time.timeScale = 0f;
        gameOverScreen.SetActive(true);
      
        // Lógica para mostrar a tela de Game Over
        Debug.Log("Ambos os jogadores estão mortos! Tela de Game Over!");
    }

    public void OnClickButtonExit()
    {
        // Reinicia o jogo recarregando a cena atual
        Time.timeScale = 1f; // Certifique-se de restaurar o tempo normal
      
        Runner.Shutdown(); // Opcionalmente, encerra o Runner antes de reiniciar
      
    }
}