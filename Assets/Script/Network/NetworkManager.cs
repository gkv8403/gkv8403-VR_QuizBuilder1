using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{

    public static NetworkManager Instance;
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform canvasTransform;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 1, 2);

    public NetworkRunner runner;
    private GameObject localPlayer;
    public bool IsHost { get; private set; }
    public bool IsClient { get; private set; }

    public List<PlayerRef> connectedPlayers = new List<PlayerRef>();
    private Dictionary<PlayerRef, string> playerNames = new Dictionary<PlayerRef, string>();

    private void Start()
    {
        DiscoverOrHost();
        SpawnLocalPlayer();
    }

    private void SpawnLocalPlayer()
    {
        if (playerPrefab == null)
        {

            Debug.LogError("[Fusion] PlayerVR Prefab is not assigned!");
            return;
        }

        Vector3 spawnPosition = canvasTransform.position + (canvasTransform.right * spawnOffset.x) +
                                (canvasTransform.up * spawnOffset.y) +
                                (canvasTransform.forward * spawnOffset.z);

        localPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.LookRotation(-canvasTransform.forward));
        Debug.Log("[Fusion] Local XR Player spawned at offset: " + spawnOffset);
    }

    private async void DiscoverOrHost()
    {
        if (runner != null)
        {
            Destroy(runner.gameObject);
        }

        runner = Instantiate(networkRunnerPrefab);
        runner.ProvideInput = true;
        runner.AddCallbacks(this);

        // Show status as searching
        UIManager.Instance.statusText.text = "Searching for Host...";

        var sessionList = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = "QuizBuilderRoom",
            PlayerCount = 10
        });

        if (sessionList.Ok)
        {
            Debug.Log("[Fusion] Joined existing host.");
            IsHost = false;
            IsClient = true;
            UIManager.Instance.CheckSession();
        }
        else
        {
            Debug.Log("[Fusion] No host found. Starting as host.");
            UIManager.Instance.statusText.text = "No Host found. You can host a game.";
        }

        await runner.Shutdown();
    }


    public async void HostGame()
    {
        // Destroy any existing NetworkRunner to avoid reuse
        if (runner != null)
        {
            Destroy(runner.gameObject);
        }

        runner = Instantiate(networkRunnerPrefab);
        runner.ProvideInput = true;
        runner.AddCallbacks(this);

        IsHost = true;
        IsClient = false;

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = "QuizBuilderRoom",
            PlayerCount = 10,
        };

        Debug.Log("[Fusion] Starting Host Session...");
        await runner.StartGame(startGameArgs);
    }

    public async void JoinSession()
    {
        // Destroy any existing NetworkRunner to avoid reuse
        if (runner != null)
        {
            Destroy(runner.gameObject);
        }

        runner = Instantiate(networkRunnerPrefab);
        runner.ProvideInput = true;
        runner.AddCallbacks(this);

        IsHost = false;
        IsClient = true;

        var joinSessionArgs = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = "QuizBuilderRoom",
        };

        Debug.Log("[Fusion] Connecting as Client...");
        await runner.StartGame(joinSessionArgs);
    }

   
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Fusion] Player left: {player}");
        connectedPlayers.Remove(player);
        UpdatePlayerListUI();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (connectedPlayers.Contains(player))
        {
            Debug.Log($"[Fusion] Player {player} already in the list, ignoring.");
            return;
        }

        connectedPlayers.Add(player);

        // Assigning a unique player name
        string uniquePlayerID = $"Player_{Guid.NewGuid().ToString().Substring(0, 8)}";

        // If it's the first player and is the host, mark them as Host
        if (IsHost && connectedPlayers.Count == 1)
        {
            uniquePlayerID = $"[Host] {uniquePlayerID}";
        }

        playerNames[player] = uniquePlayerID;

        Debug.Log($"[Fusion] {uniquePlayerID} joined the game.");
        UpdatePlayerListUI();
    }
    public void UpdatePlayerListUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerList(connectedPlayers, playerNames);
        }
    }
    public string GetPlayerName(PlayerRef player)
    {
        if (playerNames.ContainsKey(player))
        {
            return playerNames[player];
        }

        return "Unknown Player";
    }

    // Required Callbacks
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject networkObject, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject networkObject, PlayerRef player) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
}
