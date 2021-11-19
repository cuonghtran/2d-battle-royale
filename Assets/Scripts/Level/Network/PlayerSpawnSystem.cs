using Mirror;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    public class PlayerSpawnSystem : NetworkBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;

        private static List<Transform> _SpawnPoints = new List<Transform>();
        private int _nextIndex = 0;

        public static void AddSpawnPoint(Transform pointTransform)
        {
            _SpawnPoints.Add(pointTransform);
            _SpawnPoints = _SpawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
        }

        public static void RemoveSpawnPoint(Transform pointTransform)
        {
            _SpawnPoints.Remove(pointTransform);
        }

        public override void OnStartServer()
        {
            MainGameNetworkManager.OnServerReadied += SpawnPlayer;
        }

        public override void OnStartClient()
        {
            InputManager.AddBlock(ActionMapNames.Player);
            InputManager._Controls.Player.Aim.Enable();
        }

        [ServerCallback]
        private void OnDestroy()
        {
            MainGameNetworkManager.OnServerReadied -= SpawnPlayer;
        }

        [Server]
        public void SpawnPlayer(NetworkConnection conn)
        {
            Transform spawnPoint = _SpawnPoints.ElementAtOrDefault(_nextIndex);
            if (spawnPoint == null)
            {
                Debug.LogError($"Missing spawn point for player {_nextIndex}.");
                return;
            }

            GameObject playerInstance = Instantiate(_playerPrefab, _SpawnPoints[_nextIndex].position, _SpawnPoints[_nextIndex].rotation);

            //spawn the player on the other objects. Conn is included to send authority.
            NetworkServer.Spawn(playerInstance, conn);

            _nextIndex++;
        }
    }
}
