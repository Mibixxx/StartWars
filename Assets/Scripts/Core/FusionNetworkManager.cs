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
    public static FusionNetworkManager Instance;
    public NetworkRunner runnerPrefab;
    private NetworkRunner runnerInstance;

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
        Debug.Log($"Player {player} joined. Total players: {runner.ActivePlayers.Count()}");

        if (runner.ActivePlayers.Count() == 2)
        {
            Debug.Log("Entrambi i giocatori connessi! Carico la scena di gioco...");

            //DeactivateAllExceptNetworkManager();

            SceneLoader.Instance.LoadScene("SinglePlayerScene");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} left.");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { request.Accept(); }
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
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

}
