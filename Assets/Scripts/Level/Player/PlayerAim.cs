using UnityEngine;
using MainGame.InputSystem;
using Mirror;

namespace MainGame
{
    public class PlayerAim : NetworkBehaviour
    {
        [SerializeField] private Transform aimTransform;

        public float aimAngle { get; private set; }

        private Vector3 _mousePosition = Vector3.zero;
        private Controls _controls;
        private Controls _Controls
        {
            get
            {
                if (_controls != null) return _controls;
                return _controls = new Controls();
            }
        }

        public override void OnStartAuthority()
        {
            enabled = true;

            _Controls.Player.Aim.performed += ctx => HandleAiming(ctx.ReadValue<Vector2>());
        }

        [ClientCallback]
        private void OnEnable()
        {
            _Controls.Enable();
        }

        [ClientCallback]
        private void OnDisable()
        {
            _Controls.Disable();
        }

        void HandleAiming(Vector2 aimAxis)
        {
            _mousePosition = UtilsClass.GetMouseWorldPositionWithZ(aimAxis, LevelManager.MainCamera);
            Vector3 aimDirection = (_mousePosition - transform.position).normalized;
            aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            aimTransform.eulerAngles = new Vector3(0, 0, aimAngle);

            // vertically flip the aim
            Vector3 aimLocalScale = Vector3.one;
            if (aimAngle > 90 || aimAngle < -90)
                aimLocalScale.y = -1f;
            else
                aimLocalScale.y = 1f;
            aimTransform.localScale = aimLocalScale;
        }
    }
}