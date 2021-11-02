using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MainGame
{
    public class NetworkGamePlayer : NetworkBehaviour
    {
        [SyncVar]
        private string _displayName = "Loading...";

        private MainGameNetworkManager _room;
        private MainGameNetworkManager _Room
        {
            get
            {
                if (_room != null) return _room;
                return _room = NetworkManager.singleton as MainGameNetworkManager;
            }
        }

        public override void OnStartClient()
        {
            DontDestroyOnLoad(gameObject);
            _Room.GamePlayers.Add(this);
        }

        public override void OnStopClient()
        {
            _Room.GamePlayers.Remove(this);
        }

        [Server]
        public void SetDisplayName(string displayName)
        {
            this._displayName = displayName;
        }
    }
}
