using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    // Singleton instance for easy access
    public static NetworkManager Instance;

    // Prefabs and UI References
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform canvasTransform; // Spawn point for players
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 1, 2); // Offset for player spawn
    [SerializeField] private GameObject StartCamera;   // Initial camera (before joining the game)

    // Cube Prefabs for the game
    [SerializeField] private GameObject redCubePrefab;
    [SerializeField] private GameObject blueCubePrefab;
    [SerializeField] private GameObject greenCubePrefab;

    // Cube Spawn Positions
    [SerializeField] private Transform redCubeSpawnPoint;
    [SerializeField] private Transform blueCubeSpawnPoint;
    [SerializeField] private Transform greenCubeSpawnPoint;

    // Network-related fields
    public NetworkRunner runner;
    public bool IsHost { get; private set; }
    public bool IsClient { get; private set; }

    // Player management
    public List<PlayerRef> connectedPlayers = new List<PlayerRef>();
    private Dictionary<PlayerRef, string> playerNames = new Dictionary<PlayerRef, string>();

    private void Awake()
    {
        // Singleton pattern for easy access
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        DiscoverOrHost(); // Automatically search or host a game
    }

    /// <summary>
    /// Automatically attempts to join an existing session. If not found, hosts a new game.
    /// </summary>
    private async void DiscoverOrHost()
    {
        UIManager.Instance.statusText.text = "Auto Searching start";

        if (runner != null)
        {
            Destroy(runner.gameObject);
        }

        // Create a new NetworkRunner instance
        runner = Instantiate(networkRunnerPrefab);
        runner.ProvideInput = true;
        runner.AddCallbacks(this);

        // Attempt to join an existing session as a client
        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = "QuizBuilderRoom",
            PlayerCount = 10
        });

        if (result.Ok)
        {
            // Successfully joined an existing session
            UIManager.Instance.statusText.text = "Joined existing host.";
            Debug.Log("[Fusion] Joined existing host.");
            IsHost = false;
            IsClient = true;
            StartCamera.SetActive(false);
        }
        else
        {
            // No session found, start as host
            UIManager.Instance.statusText.text = "No host found. Starting as host.";
            Debug.Log("[Fusion] No host found. Starting as host.");
            HostGame();
        }
    }

    /// <summary>
    /// Hosts a new game session.
    /// </summary>
    public async void HostGame()
    {
        if (runner != null) Destroy(runner.gameObject);

        runner = Instantiate(networkRunnerPrefab);
        runner.ProvideInput = true;
        runner.AddCallbacks(this);

        IsHost = true;
        IsClient = false;

        await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = "QuizBuilderRoom",
            PlayerCount = 10
        });

        // Only host can spawn cubes
        SpawnNetworkedCubes();
    }

    /// <summary>
    /// Spawns networked cubes for the game (host only).
    /// </summary>
    private void SpawnNetworkedCubes()
    {
        if (!IsHost) return;

        SpawnCubeAtPosition(redCubePrefab, redCubeSpawnPoint.position, redCubeSpawnPoint.rotation);
        SpawnCubeAtPosition(blueCubePrefab, blueCubeSpawnPoint.position, blueCubeSpawnPoint.rotation);
        SpawnCubeAtPosition(greenCubePrefab, greenCubeSpawnPoint.position, greenCubeSpawnPoint.rotation);
    }

    /// <summary>
    /// Spawns a cube at a specified position (host only).
    /// </summary>
    private void SpawnCubeAtPosition(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (runner == null || prefab == null)
        {
            Debug.LogError("[Fusion] NetworkRunner or prefab is missing.");
            return;
        }

        var cube = runner.Spawn(prefab, position, rotation);
        if (cube == null)
            Debug.LogError("Cube failed to spawn.");
        else
            Debug.Log($"Spawned cube at: {position}");
    }

    /// <summary>
    /// Called when a player joins the game.
    /// </summary>
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("[Fusion] Player joined: " + player);

        if (!connectedPlayers.Contains(player))
        {
            connectedPlayers.Add(player);
            string uniquePlayerID = $"Player_{player.PlayerId}";

            if (IsHost && player == runner.LocalPlayer)
                uniquePlayerID = $"[Host] {uniquePlayerID}";

            playerNames[player] = uniquePlayerID;
            UpdatePlayerListUI();

            if (IsHost)
            {
                SpawnNetworkPlayer(player, uniquePlayerID);
                StartCamera.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Spawns a networked player object (host only).
    /// </summary>
    private void SpawnNetworkPlayer(PlayerRef player, string playerName)
    {
        Vector3 spawnPosition = canvasTransform.position +
                                (canvasTransform.right * spawnOffset.x) +
                                (canvasTransform.up * spawnOffset.y) +
                                (canvasTransform.forward * spawnOffset.z * connectedPlayers.Count);

        var networkObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        var networkedPlayer = networkObject.GetComponent<NetworkedPlayer>();

        if (networkedPlayer != null)
        {
            networkedPlayer.SetPlayerName(playerName);
        }
        else
        {
            Debug.LogError("[Fusion] NetworkedPlayer script missing on Player Prefab.");
        }
    }

    /// <summary>
    /// Updates the player list in the UI.
    /// </summary>
    public void UpdatePlayerListUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerList(connectedPlayers, playerNames);
        }
    }

    /// <summary>
    /// Gets the player's name using their PlayerRef.
    /// </summary>
    public string GetPlayerName(PlayerRef player)
    {
        if (playerNames.ContainsKey(player))
            return playerNames[player];

        return "Unknown Player";
    }

    /// <summary>
    /// Shuts down the NetworkRunner instance.
    /// </summary>
    public void ShutdownRunner()
    {
        if (runner != null)
        {
            runner.Shutdown();
            Destroy(runner.gameObject);
            runner = null;
            Debug.Log("[Fusion] NetworkRunner successfully shut down.");
        }
    }

    /// <summary>
    /// Called when a player leaves the game.
    /// </summary>
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Fusion] Player left: {player}");
        connectedPlayers.Remove(player);
        playerNames.Remove(player);
        UpdatePlayerListUI();
    }

    // Fusion required callbacks (unused)
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        runner = null;
        Debug.Log("[Fusion] Runner shut down with reason: " + shutdownReason);
    }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject networkObject, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject networkObject, PlayerRef player) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
}