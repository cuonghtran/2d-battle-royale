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
            public GameObject damager;
            public string sourcePlayer;
            public float amount;
            public Vector3 direction;
            public float knockBackForce;
        }

        public float maxHitPoints;
        public float maxArmor;

        [SyncVar(hook = nameof(OnHitPointsChanged))]
        private float _currentHitPoints;
        public float CurrentHitPoints { get { return _currentHitPoints; } }
        [SyncVar(hook = nameof(OnArmorChanged))]
        private float _currentArmor;
        public float CurrentArmor { get { return _currentArmor; } }

        public float invulnerabilityTime = 0f;
        public bool isInvulnerable;
        public GameObject damagePopupPrefab;

        public UnityEvent OnDeath, OnReceiveDamage, OnBecomeVulnerable;
        public Action<Damageable> OnHealthChanged;

        protected float _timeSinceLastHit = 0.0f;
        private Collider _collider;
        private SpriteRenderer _sRenderer;

        #region Server

        public override void OnStartClient()
        {
            _collider = GetComponent<Collider>();
            _sRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            ResetDamageable();
        }

        [Server]
        private IEnumerator SetDamage(DamageMessage dmgMessage)
        {
            yield return CalculateDamageDone(dmgMessage.amount);
        }

        #endregion

        #region Client

        //private void Update()
        //{
            // if (!hasAuthority) return;

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

        private void OnHitPointsChanged(float oldValue, float newValue)
        {
            OnHealthChanged?.Invoke(this);
        }

        private void OnArmorChanged(float oldValue, float newValue)
        {
            OnHealthChanged?.Invoke(this);
        }

        #endregion

        private void ResetDamageable()
        {
            _currentHitPoints = maxHitPoints;
            _currentArmor = maxArmor;
            isInvulnerable = false;
            _timeSinceLastHit = 0f;
        }

        public void SetColliderState(bool enabled)
        {
            _collider.enabled = enabled;
        }

        public void ApplyDamage(DamageMessage dmgMessage)
        {
            // already dead or invulnerable
            if (_currentHitPoints <= 0 || isInvulnerable) return;

            StartCoroutine(SetDamage(dmgMessage));
            TargetShowDamagePopupText(dmgMessage.amount);

            //if (_currentHitPoints <= 0)
            //{
            //    isInvulnerable = true;
            //    CmdUpdatePlayerPoint(dmgMessage.sourcePlayer);
            //    CmdRespawn();
            //}
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

        [TargetRpc]
        void TargetShowDamagePopupText(float dmg)
        {
            var topMostPos = transform.GetComponent<SpriteRenderer>().bounds.size.y / 2;
            Vector3 dmgPos = new Vector3(transform.position.x, transform.position.y + topMostPos, transform.position.z);

            var dmgText = Instantiate(damagePopupPrefab, dmgPos, Quaternion.identity);
            dmgText.GetComponent<DamagePopup>().SetUp(dmg, true);
        }

        [Command]
        private void CmdUpdatePlayerPoint(string sourcePlayer)
        {
            NetworkGamePlayer.singleton.UpdatePlayerPoints(sourcePlayer);
        }

        [Command]
        private void CmdRespawn()
        {
            var playerNetwork = GetComponent<PlayerNetwork>();
            LevelManager.singleton.ServerRespawn(playerNetwork);
        }
    }
}