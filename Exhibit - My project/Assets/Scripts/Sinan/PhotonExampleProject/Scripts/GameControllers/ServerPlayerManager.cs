using System.Collections.Generic;
using UnityEngine;
using Fusion.Sockets;
using System;
using ExitGames.Client.Photon.StructWrapping;
using Unity.VisualScripting;

namespace Fusion.Sample.DedicatedServer
{
    [SimulationBehaviour(Modes = SimulationModes.Server)]
    public class ServerPlayerManager : SimulationBehaviour, INetworkRunnerCallbacks
    {
        // public struct Party : INetworkStruct
        // {
        //     //public List<PlayerRef> playersInParty { get; set; }
        //     [Networked] [Capacity(6)] public NetworkLinkedList<PlayerRef> playersInParty { get; }
        // }

        [SerializeField] private NetworkObject _playerPrefab;

        private readonly Dictionary<PlayerRef, NetworkObject> _playerMap = new Dictionary<PlayerRef, NetworkObject>();
        NetworkPlayer.Party tempParty = new NetworkPlayer.Party();

        //public List<PlayerRef> inRoomPlayers = new List<PlayerRef>();
        private Dictionary<int, NetworkPlayer.Party> playerRoomsDictionary = new Dictionary<int, NetworkPlayer.Party>();
        public GameObject hoverBoard;

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer && _playerPrefab != null)
            {
                var pos = UnityEngine.Random.insideUnitSphere * 3;
                pos.y = 1;

                var character = runner.Spawn(_playerPrefab, pos, Quaternion.identity, inputAuthority: player);
                character.GetComponent<NetworkPlayer>().serverPlayerManager = this;
                _playerMap[player] = character;

                Log.Info($"Spawn for Player: {player}");
            }
        }

        private void Update()
        {
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_playerMap.TryGetValue(player, out var character))
            {
                // Despawn Player
                runner.Despawn(character);

                // Remove player from mapping
                _playerMap.Remove(player);

                Log.Info($"Despawn for Player: {player}");
            }

            // if (_playerMap.Count == 0)
            // {
            //     Log.Info("Last player left, shutdown...");
            //
            //     // Shutdown Server after the last player leaves
            //     runner.Shutdown();
            // }
        }

        public void PlayerJoinedToRoom(int roomId, PlayerRef playerRef)
        {
            Debug.LogWarning($"Player Joined to room {roomId} playerAdded to the party");
            if (playerRoomsDictionary.TryGetValue(roomId, out var value))
            {
                Debug.Log($"room {roomId} has {playerRoomsDictionary[roomId].playersInParty.Count} players");
                tempParty = playerRoomsDictionary[roomId];
                tempParty.playersInParty.Add(playerRef);
                playerRoomsDictionary[roomId] = tempParty;
                Debug.Log($"incoming player added to the room btw player ref {playerRef}");
            }
            else
            {
                tempParty.playersInParty.Clear();
                tempParty.playersInParty.Add(playerRef);
                Debug.Log($"incoming player added to the room btw player ref {playerRef}");
                playerRoomsDictionary.Add(roomId, tempParty);

                playerRoomsDictionary.TryGetValue(roomId, out var kalue);
                Debug.Log($"Room id was Created   +++++ room {roomId} has {kalue.playersInParty.Count} players");
            }

            SendAllPartyMembersToGroup(roomId);
        }

        public void PlayerQuitRoom(int roomId, PlayerRef playerRef)
        {
            tempParty = playerRoomsDictionary[roomId];
            tempParty.playersInParty.Remove(playerRef);
            playerRoomsDictionary[roomId] = tempParty;

            SendAllPartyMembersToGroup(roomId);
        }

        private void SendAllPartyMembersToGroup(int roomId)
        {
            for (int i = 0; i < playerRoomsDictionary[roomId].playersInParty.Count; i++)
            {
                GetPlayersInRoom(playerRoomsDictionary[roomId].playersInParty[i], playerRoomsDictionary[roomId]);
            }
        }

        public void GetPlayersInRoom(PlayerRef playerRef, NetworkPlayer.Party party)
        {
            Runner.TryGetPlayerObject(playerRef, out var plObject);
            plObject.GetComponent<NetworkPlayer>().myParty = party;
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("bağlandı");
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
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

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            // Quit application after the Server Shutdown
            Application.Quit(0);
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }
    }
}