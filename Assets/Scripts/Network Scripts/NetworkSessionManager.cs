using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace Network_Scripts
{
    public class NetworkSessionManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static NetworkSessionManager Instance { get; private set; }
    
        [SerializeField] private GameMode gameMode = GameMode.AutoHostOrClient;
        [SerializeField] private string sessionName = "TestRoom";

        public NetworkRunner Runner { get; set; }
        public bool IsSessionReady { get; private set; }
    
        public System.Action OnSessionStarted;

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

        private void Start()
        {
            StartCoroutine(StartSession());
        }

        private IEnumerator StartSession()
        {
            Debug.Log("Starting Fusion session...");
        
            Runner = gameObject.AddComponent<NetworkRunner>();
            Runner.ProvideInput = true;
        
            // Start the game and get the task
            var startTask = Runner.StartGame(new StartGameArgs()
            {
                GameMode = gameMode,
                SessionName = sessionName,
                Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex),
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        
            // Wait for task to complete
            while (!startTask.IsCompleted)
            {
                yield return null;
            }
        
            var result = startTask.Result;
        
            if (result.Ok)
            {
                Debug.Log("✅ Fusion session started!");
                Debug.Log($"State: IsRunning={Runner.IsRunning}, IsServer={Runner.IsServer}, IsClient={Runner.IsClient}");
            
                IsSessionReady = true;
                OnSessionStarted?.Invoke();
            }
            else
            {
                Debug.LogError($"❌ Failed to start session: {result.ShutdownReason}");
            }
        }
    
        // INetworkRunnerCallbacks (same as before)
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
        {
            Debug.Log($"Player joined: {player}");
        }
    
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    }
}