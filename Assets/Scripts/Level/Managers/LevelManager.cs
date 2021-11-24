using System;
using UnityEngine;
using Mirror;

namespace MainGame
{
    public class LevelManager : NetworkBehaviour
    {
        public static LevelManager singleton;

        private static Camera _mainCamera;
        public static Camera MainCamera
        {
            get
            {
                if (!_mainCamera) _mainCamera = Camera.main;
                return _mainCamera;
            }
        }

        public GameObject playerObjectPrefab;

        private void Awake()
        {
            if (singleton == null)
                singleton = this;
        }

        public void ServerRespawn(PlayerNetwork oldPlayer)
        {
            var conn = oldPlayer.connectionToClient;
            //var newPlayer = Instantiate(playerObjectPrefab);
            NetworkGamePlayer.singleton.RespawnCall(conn);
            Destroy(oldPlayer.gameObject);
            // NetworkServer.ReplacePlayerForConnection(conn, newPlayer, true);
        }
    }
}