using UnityEngine;

namespace MainGame
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private MainGameNetworkManager _networkManagerLobby;

        [Header("UI")]
        [SerializeField] private GameObject _landingPagePanel;

        public void HostLobbyButton_Click()
        {
            _networkManagerLobby.StartHost();
            _landingPagePanel.SetActive(false);
        }
    }
}
