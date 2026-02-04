#region Assemblies
//UI Script of the login Scene
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
#endregion

public class HostClientScript : NetworkBehaviour, INetworkRunnerCallbacks
{
    #region Variables
    //public(s)
    [SerializeField]public InputField hostInputField;
    [SerializeField]public InputField joinInputField;
    [SerializeField]public Button HostBtn;
    [SerializeField]public Button JoinBtn;
    //private(s)
    #endregion
    #region Methods
    //onbtn click
    void Start()
    {
        Button _hostbtn = HostBtn.GetComponent<Button>();
Button _joinbtn = JoinBtn.GetComponent<Button>();
        _hostbtn.onClick.AddListener(SetLobbyName);
_joinbtn.onClick.AddListener(JoinRoom);
    }
    #endregion

    //if player hosts the room
    public void SetLobbyName()
    {
        if (!HasInputAuthority)
        {
            string lobbyName = hostInputField.text;
            StartGame(lobbyName);
            Debug.Log("ThisBtnWorks");
        }
    }

    public void StartGame(string lobbyName)
    {
        if (!HasInputAuthority)
        {
            var sceneManager = new SceneManage();
            Runner.StartGame(new StartGameArgs
            {
                SessionName = lobbyName,
                IsOpen = true,
                IsVisible = true,
                SceneManager = sceneManager,
            });
//Delete comment if working
Debug.Log("StartGame(lobbyName) is Functionable");
        }
    }
    //if player is joining the host's room
    public void JoinRoom()
    {
        if (HasInputAuthority)
        {
            Task.Run(async () => await JoinLobby());
        }
    }
    
    public async Task JoinLobby()
    {
        string lobbyName = hostInputField.text;
        string customname = joinInputField.text;
        if (lobbyName == customname)
        {
            var result = await Runner.JoinSessionLobby(SessionLobby.Custom, hostInputField.text);
            if (result.Ok) {
                // all good
            } else {
                Debug.LogError($"Failed to Start: {result.ShutdownReason}");
            }
        }
    }
    #region INetworkRunnerCallbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
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
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){ }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }
    #endregion
}
