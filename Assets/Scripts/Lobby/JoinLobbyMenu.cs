using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MainGame
{
    public class JoinLobbyMenu : MonoBehaviour
    {
        [SerializeField] private MainGameNetworkManager _networkManagerLobby;

        [Header("UI")]
        [SerializeField] private GameObject _landingPagePanel;
        [SerializeField] private TMP_InputField _ipAddressInputField;
        [SerializeField] private Button _joinButton;

        private void OnEnable()
        {
            MainGameNetworkManager.OnClientConnected += HandleClientConnected;
            MainGameNetworkManager.OnClientDisconnected += HandleClientDisconnected;
        }

        private void OnDisable()
        {
            MainGameNetworkManager.OnClientConnected -= HandleClientConnected;
            MainGameNetworkManager.OnClientDisconnected -= HandleClientDisconnected;
        }

        public void JoinLobbyButton_Click()
        {
            string ipAddress = _ipAddressInputField.text;

            _networkManagerLobby.networkAddress = ipAddress;
            _networkManagerLobby.StartClient();

            _joinButton.interactable = false;
        }

        private void HandleClientConnected()
        {
            _joinButton.interactable = true;

            gameObject.SetActive(false);
            _landingPagePanel.SetActive(false);
        }

        private void HandleClientDisconnected()
        {
            _joinButton.interactable = true;
        }
    }
}
