using Mirror;
using System.Linq;
using UnityEngine;

namespace MainGame
{
    public class RoundSystem : NetworkBehaviour
    {
        [SerializeField] private Animator _animator;

        private MainGameNetworkManager _room;
        private MainGameNetworkManager _Room
        {
            get
            {
                if (_room != null) return _room;
                return _room = NetworkManager.singleton as MainGameNetworkManager;
            }
        }

        public void CountdownEnded()
        {
            _animator.enabled = false;
        }

        #region Server

        public override void OnStartServer()
        {
            MainGameNetworkManager.OnServerReadied += CheckToStartRound;
            MainGameNetworkManager.OnServerStopped += CleanUpServer;
        }

        [ServerCallback]
        private void OnDestroy()
        {
            CleanUpServer();
        }

        [Server]
        private void CleanUpServer()
        {
            MainGameNetworkManager.OnServerReadied -= CheckToStartRound;
            MainGameNetworkManager.OnServerStopped -= CleanUpServer;
        }

        [Server]
        private void CheckToStartRound(NetworkConnection conn)
        {
            // if the num of ready players != the num of total players
            if (_Room.GamePlayers.Count(x => x.connectionToClient.isReady) != _Room.GamePlayers.Count)
                return;
            
            _animator.enabled = true;  // all players are ready

            RpcStartCountdown();
        }

        [ServerCallback]
        public void StartRound()
        {
            RpcStartRound();
        }

        #endregion

        #region Client

        [ClientRpc]
        private void RpcStartCountdown()
        {
            _animator.enabled = true;
        }

        [ClientRpc]
        private void RpcStartRound()
        {
            InputManager.RemoveBlock(ActionMapNames.Player);
        }

        #endregion
    }
}
