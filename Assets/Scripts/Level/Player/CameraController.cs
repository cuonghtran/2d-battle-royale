using Mirror;
using UnityEngine;
using Cinemachine;
using MainGame.InputSystem;

namespace MainGame
{
    public class CameraController : NetworkBehaviour
    {
        [Header("Camera")]
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;

        public override void OnStartAuthority()
        {
            _virtualCamera.gameObject.SetActive(true);
        }
    }
}
