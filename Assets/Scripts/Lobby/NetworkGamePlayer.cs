using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using System.Linq;

namespace MainGame
{
    public class NetworkGamePlayer : NetworkBehaviour
    {
        public static NetworkGamePlayer singleton;

        [SyncVar]
        private string _displayName = "Loading...";

        [Header("UI")]
        [SerializeField] private GameObject _leaderBoardCanvas;
        [SerializeField] private TMP_Text[] _playerNameTexts = new TMP_Text[4];
        [SerializeField] private TMP_Text[] _playerPointsTexts = new TMP_Text[4];

        private MainGameNetworkManager _room;
        private MainGameNetworkManager _Room
        {
            get
            {
                if (_room != null) return _room;
                return _room = NetworkManager.singleton as MainGameNetworkManager;
            }
        }

        public override void OnStartAuthority()
        {
            _leaderBoardCanvas.SetActive(true);
        }

        public override void OnStartClient()
        {
            if (singleton == null && isLocalPlayer)
                singleton = this;

            DontDestroyOnLoad(gameObject);
            _Room.GamePlayers.Add(this);
            _Room.PlayerPoints[_displayName] = 0;
            UpdateLeaderboard();
        }

        public override void OnStopClient()
        {
            _Room.GamePlayers.Remove(this);
            UpdateLeaderboard();
        }

        [Server]
        public void SetDisplayName(string displayName)
        {
            _displayName = displayName;
        }

        public string GetDisplayName()
        {
            return _displayName;
        }

        private void UpdateLeaderboard()
        {
            if (!hasAuthority)
            {
                foreach (var player in _Room.GamePlayers)
                {
                    if (player.hasAuthority)
                    {
                        player.UpdateLeaderboard();
                        break;
                    }
                }

                return;
            }
            // clear the UI text
            for (int i = 0; i < _playerNameTexts.Length; i++)
            {
                _playerNameTexts[i].text = "";
                _playerPointsTexts[i].text = string.Empty;
            }

            for (int i = 0; i < _Room.GamePlayers.Count; i++)
            {
                _playerNameTexts[i].text = _Room.GamePlayers[i]._displayName;
                _playerPointsTexts[i].text = _Room.PlayerPoints[_Room.GamePlayers[i]._displayName].ToString();
            }
        }

        [Server]
        public void UpdatePlayerPoints(string playerName)
        {
            _Room.PlayerPoints[playerName] += 1;
            UpdateLeaderboard();
        }

        public void RespawnCall(NetworkConnection conn)
        {
            _Room.Respawn(conn);
        }
    }
}
