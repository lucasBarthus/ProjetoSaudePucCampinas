using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;
using static Unity.Collections.Unicode;

public class FusionManager : MonoBehaviour, INetworkRunnerCallbacks
{

    public static bool sessionCreated = false;

    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public static NetworkRunner runnerInstance;
    public string lobbyName = "Default";
    public Transform sessionListContentParent;
    public GameObject sessionListEntryPrefab;
    public Dictionary<string, GameObject> sessionListUiDictionary = new Dictionary<String, GameObject>();

    private int reconnectAttempts = 0;
    private const int maxReconnectAttempts = 3;

    public string lobbyGameSceneName;
    public GameObject botaoCriacao;

    private void Awake()
    {
        runnerInstance = gameObject.GetComponent<NetworkRunner>();

        if (runnerInstance == null)
        {
            runnerInstance = gameObject.AddComponent<NetworkRunner>();
        }
    }

    private void Start()
    {
        runnerInstance.JoinSessionLobby(SessionLobby.Shared, lobbyName);
    }

    public void CreatedRandomSession()
    {
        FusionManager.sessionCreated = true;
        int randomInt = UnityEngine.Random.Range(1000, 9999);
        string randomSessionName = "Room" + randomInt.ToString();

        runnerInstance.StartGame(new StartGameArgs()
        {
            Scene = SceneRef.FromIndex(GetSceneIndex(lobbyGameSceneName)),
            SessionName = randomSessionName,
            GameMode = GameMode.Host // O servidor (host) deve criar a sess�o e instanciar o PlayerPrefab
        });
    }

    public void JoinSession(string sessionName)
    {
        if (runnerInstance != null)
        {
            Debug.Log($"Tentando entrar na sess�o: {sessionName}");

            runnerInstance.StartGame(new StartGameArgs()
            {
                SessionName = sessionName,
                GameMode = GameMode.Client, // Define como Client para entrar em uma sess�o existente.
                Scene = SceneRef.None // Use None se a cena j� estiver carregada, ou use SceneRef se precisar carregar uma cena espec�fica.
            });
        }
        else
        {
            Debug.LogError("runnerInstance n�o est� inicializado.");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return; // Garante que apenas o servidor realize a inst�ncia

        // Verifica se o jogador j� foi instanciado para este player
        if (_spawnedCharacters.ContainsKey(player))
            return;

        // Defina os limites da �rea onde voc� deseja que o jogador apare�a
        float minX = -5f;
        float maxX = 5f;
        float minZ = -5f;
        float maxZ = 5f;

        // Gera uma posi��o aleat�ria dentro dos limites especificados
        float randomX = UnityEngine.Random.Range(minX, maxX);
        float randomZ = UnityEngine.Random.Range(minZ, maxZ);
        Vector3 spawnPosition = new Vector3(randomX, 1, randomZ);

        // Instancia o objeto de rede do jogador com autoridade de input para o cliente espec�fico
        NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);

        // Mant�m o controle dos avatares dos jogadores para acesso f�cil
        _spawnedCharacters.Add(player, networkPlayerObject);
    }

    /*
    Metodo para atualizar  a posi��o frame a frame (n�o esta sendo utilizado)
    private void UpdatePlayerPosition(NetworkObject networkPlayerObject, Vector3 position)
    {
        // Acessa o Transform do NetworkObject e atualiza a posi��o
        Transform playerTransform = networkPlayerObject.transform;
        playerTransform.position = position;
    }*/

    public int GetSceneIndex(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (name == sceneName)
            {
                return i;
            }
        }
        return -1;
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Player connected to the server");
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        DeleteOldSessionsFromUI(sessionList);
        CompareList(sessionList);
    }

    private void DeleteOldSessionsFromUI(List<SessionInfo> sessionList)
    {
        bool isContained = false;
        GameObject uiToDelete = null;

        foreach (KeyValuePair<string, GameObject> kvp in sessionListUiDictionary)
        {
            string sessionKey = kvp.Key;

            foreach (SessionInfo sessionInfo in sessionList)
            {
                if (sessionInfo.Name == sessionKey)
                {
                    isContained = true;
                    break;
                }
            }

            if (!isContained)
            {
                uiToDelete = kvp.Value;
                sessionListUiDictionary.Remove(sessionKey);
                Destroy(uiToDelete);
            }
        }
    }

    private void CompareList(List<SessionInfo> sessionList)
    {
        foreach (SessionInfo session in sessionList)
        {
            if (sessionListUiDictionary.ContainsKey(session.Name))
            {
                UpdateEntryUI(session);
            }
            else
            {
                CrateEntryUi(session);
            }
        }
    }

    private void CrateEntryUi(SessionInfo session)
    {
        GameObject newEntry = GameObject.Instantiate(sessionListEntryPrefab);
        newEntry.transform.parent = sessionListContentParent;
        SessionListPrefeb entryScript = newEntry.GetComponent<SessionListPrefeb>();
        sessionListUiDictionary.Add(session.Name, newEntry);

        entryScript.roomName.text = session.Name;
        entryScript.playerCount.text = session.PlayerCount.ToString() + "/" + session.MaxPlayers.ToString();
        entryScript.joinButto.interactable = session.IsOpen;

        newEntry.SetActive(session.IsVisible);
    }

    private void UpdateEntryUI(SessionInfo session)
    {
        sessionListUiDictionary.TryGetValue(session.Name, out GameObject newEntry);

        if (newEntry != null)
        {
            SessionListPrefeb entryScript = newEntry.GetComponent<SessionListPrefeb>();
            entryScript.roomName.text = session.Name;
            entryScript.playerCount.text = session.PlayerCount.ToString() + "/" + session.MaxPlayers.ToString();
            entryScript.joinButto.interactable = session.IsOpen;

            newEntry.SetActive(session.IsVisible);
        }
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"Failed to connect to server: {reason}");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        // Handle custom authentication if needed
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        // Handle custom authentication response if needed
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"Disconnected from server: {reason}");

        if (reconnectAttempts < maxReconnectAttempts)
        {
            reconnectAttempts++;
            Debug.Log($"Attempting to reconnect... Attempt {reconnectAttempts}/{maxReconnectAttempts}");
            runnerInstance.StartGame(new StartGameArgs()
            {
                SessionName = runner.SessionInfo.Name,
                GameMode = runner.GameMode,
                Scene = SceneRef.None // Assume the scene is already loaded
            });
        }
        else
        {
            Debug.LogError("Max reconnection attempts reached. Failed to reconnect.");
            // Handle max reconnect attempts reached (e.g., notify player, return to main menu)
        }
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        // Handle host migration if needed
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        // Coleta de entrada para movimento em 2D
        if (Input.GetKey(KeyCode.W))
            data.direction += Vector2.up;

        if (Input.GetKey(KeyCode.S))
            data.direction += Vector2.down;

        if (Input.GetKey(KeyCode.A))
            data.direction += Vector2.left;

        if (Input.GetKey(KeyCode.D))
            data.direction += Vector2.right;

        // Coleta de entrada para disparo
        data.fire = Input.GetKey(KeyCode.Space);

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        // Handle missing input if needed
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        // Handle object entering Area of Interest if needed
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        // Handle object exiting Area of Interest if needed
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        // Handle reliable data progress if needed
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        // Handle reliable data received if needed
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer) return; // Apenas o servidor deve instanciar o jogador

        // Verifica se o jogador local j� tem um objeto associado
        if (runner.GetPlayerObject(runner.LocalPlayer) == null && !_spawnedCharacters.ContainsKey(runner.LocalPlayer))
        {
            Vector3 spawnPosition = new Vector3(0, 1, 0); // Define a posi��o de spawn necess�ria
            NetworkObject playerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, runner.LocalPlayer);
            _spawnedCharacters.Add(runner.LocalPlayer, playerObject);
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        // Handle scene load start if needed
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        // Handle shutdown if needed
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        // Handle user simulation messages if needed
    }

    public enum SessionState
    {
        NoLobby,
        InLobby,
        Loading,
        InSession
    }

    public SessionState CurrentSessionState { get; private set; }

    private void SetSessionState(SessionState newState)
    {
        CurrentSessionState = newState;
        // Realize a��es baseadas no estado, se necess�rio.
    }

    void Update()
    {
        if (sessionCreated)
        {
            return; // Sai do m�todo Update e n�o executa o restante do c�digo
        }

        if (runnerInstance.IsCloudReady)
        {


            // Ativa o GameObject "botaoCriacao" se estiver atribu�do
            if (botaoCriacao != null)
            {
                botaoCriacao.SetActive(true);
            }
            else
            {

            }
        }
        else
        {
            Debug.LogWarning("NetworkRunner n�o est� pronto para a nuvem.");
        }
    }

}