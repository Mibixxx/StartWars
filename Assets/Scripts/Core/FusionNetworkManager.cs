using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Linq;

public class FusionNetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static FusionNetworkManager Instance { get; private set; }

    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private GameObject playerPrefab; // Il prefab del giocatore di rete

    private NetworkRunner runnerInstance;
    private readonly Dictionary<PlayerRef, Player> _spawnedPlayers = new Dictionary<PlayerRef, Player>();


    private void Awake()
    {
        // Mantieni il manager tra le scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPlayerToList(Player player)
    {
        if (!_spawnedPlayers.ContainsKey(player.PlayerRef))
        {
            _spawnedPlayers.Add(player.PlayerRef, player);
        }
    }

    public void RemovePlayerFromList(Player player)
    {
        if (_spawnedPlayers.ContainsKey(player.PlayerRef))
        {
            _spawnedPlayers.Remove(player.PlayerRef);
        }
    }

    public Player GetPlayer(PlayerRef playerRef)
    {
        _spawnedPlayers.TryGetValue(playerRef, out var player);
        return player;
    }

    public NetworkRunner GetRunner() => runnerInstance;

    public void DeactivateAllExceptNetworkManager()
    {
        // Prendi la scena attiva
        Scene activeScene = SceneManager.GetActiveScene();

        // Prendi tutti gli oggetti root della scena attiva
        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        foreach (var go in rootObjects)
        {
            // Se questo oggetto ha il FusionNetworkManager, skippa
            if (go.GetComponent<FusionNetworkManager>() != null)
                continue;

            // Altrimenti disattivalo
            go.SetActive(false);
        }
    }

    private int GetSceneBuildIndexByName(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return i;
        }

        Debug.LogError($"La scena '{sceneName}' non � stata trovata nei Build Settings.");
        return -1;
    }

    public async Task StartSinglePlayerGame(string sceneName)
    {
        if (runnerInstance == null)
        {
            runnerInstance = Instantiate(runnerPrefab);
            runnerInstance.AddCallbacks(this);

            var result = await runnerInstance.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Single,
                SessionName = "OfflineSession",
                Address = NetAddress.Any()
            });

            Debug.Log($"StartGame result: {result.Ok}, Error: {result.ShutdownReason}");
        }

        SceneLoader.Instance.LoadScene(sceneName);
    }

    public async Task StartOnlineGame()
    {
        if (runnerInstance == null)
            runnerInstance = Instantiate(runnerPrefab);

        runnerInstance.AddCallbacks(this);

        // Tenta di unirti a una partita esistente, se non c'�, crea una nuova sessione
        var result = await runnerInstance.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient, // Tenta host se nessuno disponibile
            SessionName = "1v1Session",
            Address = NetAddress.Any()
        });

        Debug.Log($"StartGame result: {result.Ok}, Error: {result.ShutdownReason}");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} joined. Total players: {runner.ActivePlayers.Count()}");

        // La logica di spawn viene eseguita solo dal server/host
        if (runner.IsServer)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("Il prefab del Player non è stato assegnato nel FusionNetworkManager!");
                return;
            }

            Debug.Log($"Spawning player object for player {player.PlayerId}");

            // Spawna il prefab del giocatore e assegna l'autorità di input al client che si è unito
            NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);

            // Associa il PlayerRef all'oggetto appena spawnato
            var playerComponent = networkPlayerObject.GetComponent<Player>();
            playerComponent.PlayerRef = player;

            // Aggiungi il giocatore appena spawnato al dizionario per tenerne traccia
            _spawnedPlayers.Add(player, playerComponent);
        }

        if (runner.ActivePlayers.Count() == 2 && runner.IsServer)
        {
            Debug.Log("Entrambi i giocatori connessi! Carico la scena di gioco...");
            runner.LoadScene("SinglePlayerScene"); // Usa runner.LoadScene per la gestione di scene in rete
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} left.");

        // Trova l'oggetto del giocatore che si è disconnesso e distruggilo
        if (_spawnedPlayers.TryGetValue(player, out Player playerObject))
        {
            playerObject.OnDisconnected(); // Chiama il metodo di pulizia sul player
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        if (runner.ActivePlayers.Count() >= 2)
        {
            // Rifiuta la connessione se la stanza è piena
            request.Refuse();
            Debug.Log("Connection refused: room is full.");
        }
        else
        {
            request.Accept();
        }
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("Session list updated:");
        foreach (var session in sessionList)
            Debug.Log($"Session: {session.Name}, Players: {session.PlayerCount}/{session.MaxPlayers}");
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("Scene load done.");
        // Qui potresti riposizionare i giocatori ai loro punti di spawn iniziali
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("Scene load start.");
    }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

}
