using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkPlayer playerPrefab;

    //Other components
    private PlayerInputs _characterInputHandler;
    private SessionListUIHandler _sessionListUIHandler;

    void Awake()
    {
        _sessionListUIHandler = FindObjectOfType<SessionListUIHandler>(true);
    }


    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            var go = runner.Spawn(playerPrefab, Singleton.Main.Utils.GetRandomSpawnPoint(),
                  Quaternion.identity,
                  player);
            Debug.Log(player.PlayerId + " joined the game");
        }
        else
        {
            Debug.Log("OnPlayerJoined");
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (_characterInputHandler == null && NetworkPlayer.Local != null)
        {
            var childCount = NetworkPlayer.Local.transform.childCount;
            NetworkPlayer.Local.transform
                .GetChild(childCount - 1).gameObject.SetActive(true);

            _characterInputHandler = NetworkPlayer.Local.transform
                .GetChild(childCount - 1).GetComponent<PlayerInputs>();
        }

        if (_characterInputHandler != null)
        {
            input.Set(_characterInputHandler.GetNetworkInput());
        }
    }

    // ReSharper disable once Unity.IncorrectMethodSignature
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("OnConnectedToServer");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;

        if (runner.TryGetPlayerObject(player, out NetworkObject playerLeftNetworkObject))
        {
            runner.Despawn(playerLeftNetworkObject);
        }
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("OnShutdown");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log("OnDisconnectedFromServer");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("OnConnectRequest");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("OnConnectFailed");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        //Only update the list of sessions when the session list UI handler is active
        if (_sessionListUIHandler == null)
            return;

        if (sessionList.Count == 0)
        {
            Debug.Log("Joined lobby no sessions found");

            _sessionListUIHandler.OnNoSessionsFound();
        }
        else
        {
            _sessionListUIHandler.ClearList();

            foreach (SessionInfo sessionInfo in sessionList)
            {
                _sessionListUIHandler.AddToList(sessionInfo);

                Debug.Log($"Found session {sessionInfo.Name} playerCount {sessionInfo.PlayerCount}");
            }
        }
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}