using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using System.Linq;
using UnityEngine.UI;


public class ReadyManager : NetworkBehaviour
{
    private NetworkRunner runner;
    public Button readyButton;
    [SerializeField] private TextMeshProUGUI playerListText;

    public GameObject spawner;
    private void Start()
    {
        runner = FusionManager.runnerInstance;
        if (runner == null)
        {
            Debug.LogError("NetworkRunner não está inicializado.");
            return;
        }
    }

    private void Update()
    {
        if (runner != null)
        {
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

        if (playerCount >= 2)
        {
            readyButton.gameObject.SetActive(true);
        }
        else
        {

            readyButton.gameObject.SetActive(false);
        }


    }

    public void OnClickReadyButton()
    {
       spawner.SetActive(true);
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
}