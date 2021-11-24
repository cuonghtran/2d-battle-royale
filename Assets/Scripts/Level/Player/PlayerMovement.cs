using MainGame.Message;
using UnityEngine;
using MainGame.InputSystem;
using Mirror;
using System;

namespace MainGame
{
    public class PlayerMovement : NetworkBehaviour, IMessageReceiver
    {
        [SerializeField] private float _speed = 5f;
        private Vector2 _movementDirection;
        private float _movementSpeed;

        private Rigidbody2D _rigidBody;
        private Animator _animator;
        private PlayerAim _playerAim;
        private SpriteRenderer _renderer;
        private Damageable _damageable;

        private enum MoveState { Normal, Rolling }

        private float _aimAngle;
        private bool _externalInputBlocked = false;
        private MoveState _moveState = MoveState.Normal;
        private float _rollSpeed;
        private float _rollSpeedDropMultiplier = 4f;
        private bool _canInteract = false;

        private int _hashHorizontal = Animator.StringToHash("Horizontal");
        private int _hashVertical = Animator.StringToHash("Vertical");
        private int _hashSpeed = Animator.StringToHash("Speed");

        public event Action<GameObject> OnPressedInteract;

        public override void OnStartAuthority()
        {
            enabled = true;
            _rigidBody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _playerAim = GetComponent<PlayerAim>();
            _renderer = transform.GetComponent<SpriteRenderer>();

            InputManager._Controls.Player.Move.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
            InputManager._Controls.Player.Move.canceled += ctx => ResetMovement();
            InputManager._Controls.Player.Roll.performed += ctx => Roll();
            InputManager._Controls.Player.Interact.performed += ctx => Interact();
        }

        [ClientCallback]
        private void OnEnable()
        {
            _damageable = GetComponent<Damageable>();
            //_damageable.isInvulnerable = true;
        }

        [ClientCallback]
        void Update()
        {
            MovementHandler();
            Animate();
        }

        [ClientCallback]
        void FixedUpdate()
        {
            switch(_moveState)
            {
                case MoveState.Normal:
                    _rigidBody.velocity = _movementDirection * _movementSpeed * _speed * Time.deltaTime;
                    break;

                case MoveState.Rolling:
                    _rigidBody.velocity = _movementDirection * _movementSpeed * _rollSpeed * Time.deltaTime;
                    break;
            }
            
        }

        [Client]
        private void SetMovement(Vector2 movement)
        {
            _movementDirection = movement.normalized;
        }

        [Client]
        private void ResetMovement()
        {
            _movementDirection = Vector2.zero;
        }

        void MovementHandler()
        {
            if (_externalInputBlocked)
            {
                _movementSpeed = 0;
                return;
            }

            switch (_moveState)
            {
                case MoveState.Normal:
                    _movementSpeed = Mathf.Clamp(_movementDirection.sqrMagnitude, 0f, 1f);
                    _movementDirection.Normalize();
                    break;

                case MoveState.Rolling:
                    _rollSpeed -= _rollSpeed * _rollSpeedDropMultiplier * Time.deltaTime;
                    float rollSpeedMinimum = 200f;
                    if (_rollSpeed < rollSpeedMinimum)
                    {
                        _animator.SetBool("IsRolling", false);
                        _moveState = MoveState.Normal;
                    }
                    break;
            }
        }

        void Roll()
        {
            if (_moveState == MoveState.Normal)
            {

                _rollSpeed = _speed * 5.8f; // starting value of roll speed
                _animator.SetBool("IsRolling", true);
                _moveState = MoveState.Rolling;
            }
        }

        void Interact()
        {
            OnPressedInteract?.Invoke(gameObject);
        }

        public void SetInteraction(bool canInteract)
        {
            _canInteract = canInteract;
        }

        void Animate()
        {
            _aimAngle = _playerAim.aimAngle;
            if (-45 < _aimAngle && _aimAngle <= 45) // face right
            {
                _animator.SetFloat(_hashHorizontal, 1);
                _animator.SetFloat(_hashVertical, 0);
                _renderer.sortingOrder = 0;
            }
            else if (45 < _aimAngle && _aimAngle <= 135) // face up
            {
                _animator.SetFloat(_hashHorizontal, 0);
                _animator.SetFloat(_hashVertical, 1);
                _renderer.sortingOrder = 3;
            }
            else if (-135 < _aimAngle && _aimAngle <= -45) // face down
            {
                _animator.SetFloat(_hashHorizontal, 0);
                _animator.SetFloat(_hashVertical, -1);
                _renderer.sortingOrder = 0;
            }
            else if (135 < _aimAngle || _aimAngle <= -135) // face left
            {
                _animator.SetFloat(_hashHorizontal, -1);
                _animator.SetFloat(_hashVertical, 0);
                _renderer.sortingOrder = 0;
            }

            _animator.SetFloat(_hashSpeed, _movementSpeed);
        }

        public bool HaveControl()
        {
            return !_externalInputBlocked;
        }

        public void BlockControl()
        {
            _externalInputBlocked = true;
        }

        public void GainControl()
        {
            _externalInputBlocked = false;
        }

        public void OnReceiveMessage(MessageType type, object sender, object msg)
        {
            throw new System.NotImplementedException();
        }
    }
}
