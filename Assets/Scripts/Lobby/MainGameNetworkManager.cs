using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections.Generic;

namespace MainGame
{
    public class MainGameNetworkManager : NetworkManager
    {
        [Header("Information")]
        [Scene] [SerializeField] private string _menuScene = string.Empty;
        [SerializeField] private int _minPlayers = 2;

        [Header("Maps")]
        [SerializeField] private int _numberOfRounds = 1;
        [SerializeField] private MapSet _mapSet;

        [Header("Room")]
        [SerializeField] private NetworkRoomPlayer _roomPlayerPrefab;

        [Header("Game")]
        [SerializeField] private NetworkGamePlayer _gamePlayerPrefab;
        [SerializeField] private GameObject _playerSpawnSystem;
        [SerializeField] private GameObject _roundSystem;
        [SerializeField] private GameObject _weaponSpawner;

        private MapHandler _mapHandler;

        public static event Action OnClientConnected;
        public static event Action OnClientDisconnected;
        public static event Action<NetworkConnection> OnServerReadied;
        public static event Action OnServerStopped;

        public List<NetworkRoomPlayer> RoomPlayers { get; } = new List<NetworkRoomPlayer>();
        public List<NetworkGamePlayer> GamePlayers { get; } = new List<NetworkGamePlayer>();
        public Dictionary<string, int> PlayerPoints = new Dictionary<string, int>();

        #region Lobby

        public override void OnStartServer()
        {
            spawnPrefabs = Resources.LoadAll<GameObject>("NetworkedPrefabs").ToList();
        }

        public override void OnStartClient()
        {
            var spawnablePrefabs = Resources.LoadAll<GameObject>("NetworkedPrefabs");
            foreach(var prefab in spawnablePrefabs)
            {
                NetworkClient.RegisterPrefab(prefab);
            }
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            OnClientConnected?.Invoke();
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            
            OnClientDisconnected?.Invoke();
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            if (numPlayers >= maxConnections)
            {
                conn.Disconnect();
                return;
            }

            if (SceneManager.GetActiveScene().path != _menuScene)
            {
                conn.Disconnect();
                return;
            }
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            if (SceneManager.GetActiveScene().path == _menuScene)
            {
                bool isLeader = RoomPlayers.Count == 0;
                NetworkRoomPlayer roomPlayerInstance = Instantiate(_roomPlayerPrefab);
                roomPlayerInstance.IsLeader = isLeader;
                NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
            }
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            if (conn.identity != null)
            {
                var player = conn.identity.GetComponent<NetworkRoomPlayer>();
                RoomPlayers.Remove(player);
                NotifyPlayersOfReadyState();
            }

            base.OnServerDisconnect(conn);
        }

        public override void OnStopServer()
        {
            OnServerStopped?.Invoke();

            RoomPlayers.Clear();
            GamePlayers.Clear();
        }

        public void NotifyPlayersOfReadyState()
        {
            foreach (var player in RoomPlayers)
            {
                player.HandleReadyToStart(IsReadyToStart());
            }
        }

        private bool IsReadyToStart()
        {
            if (numPlayers < _minPlayers)
                return false;

            foreach(var player in RoomPlayers)
            {
                if (!player.IsReady)
                    return false;
            }

            return true;
        }

        #endregion

        #region Game

        public void StartGame()
        {
            if (SceneManager.GetActiveScene().path == _menuScene)
            {
                if (!IsReadyToStart()) return;

                _mapHandler = new MapHandler(_mapSet, _numberOfRounds);

                ServerChangeScene(_mapHandler.NextMap());
            }
        }

        public override void ServerChangeScene(string newSceneName)
        {
            // from menu to game
            if (SceneManager.GetActiveScene().path == _menuScene && newSceneName.Contains("Scene_Level"))
            {
                for (int i = RoomPlayers.Count - 1; i >=0; i--)
                {
                    var conn = RoomPlayers[i].connectionToClient;
                    var gamePlayerInstance = Instantiate(_gamePlayerPrefab);
                    gamePlayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);
                    NetworkServer.Destroy(conn.identity.gameObject);
                    NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject, true);
                }
            }

            base.ServerChangeScene(newSceneName);
        }

        public override void OnServerChangeScene(string newSceneName)
        {
            // TODO: might do the scene transition effect here

            base.OnServerChangeScene(newSceneName);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            if (sceneName.Contains("Scene_Level"))
            {
                GameObject playerSpawnSystemInstance = Instantiate(_playerSpawnSystem);
                NetworkServer.Spawn(playerSpawnSystemInstance);

                GameObject roundSystemInstance = Instantiate(_roundSystem);
                NetworkServer.Spawn(roundSystemInstance);

                GameObject weaponSpawnerInstance = Instantiate(_weaponSpawner);
                NetworkServer.Spawn(weaponSpawnerInstance);
            }
        }

        public override void OnServerReady(NetworkConnection conn)
        {
            base.OnServerReady(conn);
            OnServerReadied?.Invoke(conn);
        }

        public void Respawn(NetworkConnection conn)
        {
            var spawner = _playerSpawnSystem.GetComponent<PlayerSpawnSystem>();
            spawner.SpawnPlayer(conn);
        }

        #endregion
    }
}
