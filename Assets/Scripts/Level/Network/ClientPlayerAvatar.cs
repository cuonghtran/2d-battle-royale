using System;
using UnityEngine;
using Mirror;

namespace MainGame
{
    public class ClientPlayerAvatar : NetworkBehaviour
    {
        [SerializeField]
        ClientPlayerAvatarRuntimeCollection _playerAvatars;

        public int connId;

        public override void OnStartAuthority()
        {
            name = "PlayerAvatar" + netId;

            if (_playerAvatars)
            {
                _playerAvatars.Add(this);
            }
        }

        public override void OnStopAuthority()
        {
            RemoveNetworkCharacter();
        }

        private void OnDestroy()
        {
            RemoveNetworkCharacter();
        }

        void RemoveNetworkCharacter()
        {
            if (_playerAvatars)
            {
                _playerAvatars.Remove(this);
            }
        }
    }
}
