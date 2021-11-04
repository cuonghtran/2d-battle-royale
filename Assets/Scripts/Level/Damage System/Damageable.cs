using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MainGame.Message;
using Mirror;
using UnityEngine.InputSystem;

namespace MainGame
{
    public class Damageable : NetworkBehaviour
    {
        public struct DamageMessage
        {
            public MonoBehaviour damager;
            public float amount;
            public Vector3 direction;
            public float knockBackForce;

            public bool stopCamera;
        }

        public float maxHitPoints;
        public float maxArmor;

        [SyncVar]
        private float _currentHitPoints;
        public float CurrentHitPoints { get { return _currentHitPoints; } }
        [SyncVar]
        private float _currentArmor;
        public float CurrentArmor { get { return _currentArmor; } }

        public float invulnerabilityTime = 0f;
        public bool isInvulnerable;

        public UnityEvent OnDeath, OnReceiveDamage, OnBecomeVulnerable, OnResetDamage;
        public Action<Damageable> OnHealthChanged;

        [Tooltip("When this gameObject is damaged, these other gameObjects are notified.")]
        public List<MonoBehaviour> onDamageMessageReceivers;

        protected float _timeSinceLastHit = 0.0f;
        protected Collider _collider;

        private Action schedule;

        #region Server

        public override void OnStartServer()
        {
            ResetDamage();
            OnHealthChanged?.Invoke(this);
        }

        [Server]
        private IEnumerator SetDamage(DamageMessage dmgMessage)
        {
            //ApplyDamage(dmgMessage);
            yield return CalculateDamageDone(dmgMessage.amount);
            RpcUpdateDamageOnClients();
        }

        [ClientRpc]
        private void RpcUpdateDamageOnClients()
        {
            OnHealthChanged?.Invoke(this);
        }

        [Command]
        private void CmdDealDamage()
        {
            var msg = new DamageMessage()
            {
                damager = this,
                amount = 30,
                direction = Vector3.up,
                stopCamera = false
            };
            StartCoroutine(SetDamage(msg));
        }

        #endregion

        #region Client

        public override void OnStartAuthority()
        {
            _collider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (!hasAuthority) return;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                CmdDealDamage();
        }

        #endregion

        //private void Update()
        //{
        //    if (!hasAuthority) return;

        //    if (isInvulnerable)
        //    {
        //        _timeSinceLastHit += Time.deltaTime;
        //        if (_timeSinceLastHit > invulnerabilityTime)
        //        {
        //            _timeSinceLastHit = 0.0f;
        //            isInvulnerable = false;
        //            OnBecomeVulnerable.Invoke();
        //        }
        //    }
        //}

        //private void LateUpdate()
        //{
        //    if (!hasAuthority) return;

        //    if (schedule != null)
        //    {
        //        schedule();
        //        schedule = null;
        //    }
        //}

        private void ResetDamage()
        {
            _currentHitPoints = maxHitPoints;
            _currentArmor = maxArmor;
            isInvulnerable = false;
            _timeSinceLastHit = 0f;
            OnResetDamage.Invoke();
        }

        public void SetColliderState(bool enabled)
        {
            _collider.enabled = enabled;
        }

        public void ApplyDamage(DamageMessage data)
        {
            // already dead or invulnerable
            if (_currentHitPoints <= 0 || isInvulnerable)
                return;

            // isInvulnerable = true;  // enable this line if want to make player invul after getting hit.
            CalculateDamageDone(data.amount);

            if (_currentHitPoints <= 0)
                schedule += OnDeath.Invoke;
            else OnReceiveDamage.Invoke();

            var messageType = _currentHitPoints <= 0 ? MessageType.DEAD : MessageType.DAMAGED;

            for (var i = 0; i < onDamageMessageReceivers.Count; ++i)
            {
                var receiver = onDamageMessageReceivers[i] as IMessageReceiver;
                receiver.OnReceiveMessage(messageType, this, data);
            }
        }

        private IEnumerator CalculateDamageDone(float amount)
        {
            if (amount <= _currentArmor)  // when the damage is less than current armor
            {
                _currentArmor = Mathf.Max(_currentArmor - amount, 0);
            }
            else  // when the damage is greater than current armor
            {
                float leftAmt = Mathf.Abs(_currentArmor - amount);
                _currentArmor = 0;
                _currentHitPoints = Mathf.Max(_currentHitPoints - leftAmt, 0);
            }

            yield return null;
        }

        //private void TEST_DAMAGE()
        //{
        //    // TEST DAMAGE
        //    if (Input.GetKeyDown(KeyCode.V))
        //    {
        //        var msg = new DamageMessage()
        //        {
        //            damager = this,
        //            amount = 35,
        //            direction = Vector3.up,
        //            stopCamera = false
        //        };
        //        ApplyDamage(msg);
        //    }
        //}

    }
}