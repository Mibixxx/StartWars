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

    public event Action<Player> OnLocalPlayerSpawned;

    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private GameObject playerPrefab;

    private NetworkRunner runnerInstance;
    private readonly Dictionary<PlayerRef, Player> _spawnedPlayers = new Dictionary<PlayerRef, Player>();


    private void Awake()
    {
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

    public Dictionary<PlayerRef, Player> GetAllPlayers()
    {
        return _spawnedPlayers;
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

    public Player GetLocalPlayer()
    {
        return _spawnedPlayers.Values.FirstOrDefault(p => p.Object.HasInputAuthority);
    }

    public NetworkRunner GetRunner() => runnerInstance;

    private int GetSceneBuildIndexByName(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return i;
        }

        Debug.LogError($"La scena '{sceneName}' non è stata trovata nei Build Settings.");
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

        var result = await runnerInstance.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "1v1Session",
            Address = NetAddress.Any()
        });

        Debug.Log($"StartGame result: {result.Ok}, Error: {result.ShutdownReason}");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} joined. Total players: {runner.ActivePlayers.Count()}");

        if (runner.IsServer)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("Il prefab del Player non Ã¨ stato assegnato nel FusionNetworkManager!");
                return;
            }

            Debug.Log($"Spawning player object for player {player.PlayerId}");

            NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);

            var playerComponent = networkPlayerObject.GetComponent<Player>();
            playerComponent.PlayerRef = player;

            _spawnedPlayers.Add(player, playerComponent);
        }

        if (runner.ActivePlayers.Count() == 2 && runner.IsServer)
        {
            Debug.Log("Entrambi i giocatori connessi! Carico la scena di gioco...");
            runner.LoadScene("MultiPlayerScene");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} left.");

        if (_spawnedPlayers.TryGetValue(player, out Player playerObject))
        {
            playerObject.OnDisconnected();
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
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("Scene load start.");
    }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

}
