using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;

namespace MainGame
{
    public class NetworkRoomPlayer : NetworkBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject lobbyUI;
        [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];
        [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[4];
        [SerializeField] private Button startGameBButton;

        [SyncVar(hook = nameof(HandleDisplayerNameChanged))]
        public string DisplayName = "Loading...";
        [SyncVar(hook = nameof(HandleReadyStatusChanged))]
        public bool IsReady = false;

        private bool _isLeader;
        public bool IsLeader
        {
            get { return _isLeader; }
            set
            {
                _isLeader = value;
                startGameBButton.gameObject.SetActive(value);
            }
        }

        private MainGameNetworkManager _room;
        private MainGameNetworkManager _Room
        {
            get
            {
                if (_room != null) return _room;
                return _room = NetworkManager.singleton as MainGameNetworkManager;
            }
        }

        public void HandleReadyToStart(bool readyToStart)
        {
            if (!_isLeader)
                return;

            startGameBButton.interactable = readyToStart;
        }

        public override void OnStartAuthority()
        {
            CmdSetDisplayName(PlayerNameInput.DisplayName);
            lobbyUI.SetActive(true);
        }

        public override void OnStartClient()
        {
            _Room.RoomPlayers.Add(this);
            UpdateDisplay();
        }

        public override void OnStopClient()
        {
            _Room.RoomPlayers.Remove(this);
            UpdateDisplay();
        }

        public void HandleDisplayerNameChanged(string oldValue, string newValue)
        {
            UpdateDisplay();
        }

        public void HandleReadyStatusChanged(bool oldValue, bool newValue)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (!hasAuthority)
            {
                foreach(var player in _Room.RoomPlayers)
                {
                    if (player.hasAuthority)
                    {
                        player.UpdateDisplay();
                        break;
                    }
                }

                return;
            }

            // clear the UI text
            for (int i = 0; i < playerNameTexts.Length; i++)
            {
                playerNameTexts[i].text = "Waiting For Player...";
                playerReadyTexts[i].text = string.Empty;
            }

            // update based on room players count
            for (int i = 0; i < _Room.RoomPlayers.Count; i++)
            {
                playerNameTexts[i].text = _Room.RoomPlayers[i].DisplayName;
                playerReadyTexts[i].text = _Room.RoomPlayers[i].IsReady ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
            }
        }

        [Command]
        private void CmdSetDisplayName(string displayName)
        {
            DisplayName = displayName;
        }

        [Command]
        public void CmdReadyUp()
        {
            IsReady = !IsReady;
            _Room.NotifyPlayersOfReadyState();
        }

        [Command]
        public void CmdStartGame()
        {
            if (_Room.RoomPlayers[0].connectionToClient != connectionToClient)
                return;

            // Start Game
            _Room.StartGame();
        }
    }
}
