using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MainGame.Message;

namespace MainGame
{
    public class Damageable : MonoBehaviour
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
        public float currentHitPoints { get; set; }
        public float maxArmor;
        public float currentArmor { get; set; }
        public float invulnerabilityTime = 0f;
        public bool isInvulnerable { get; set; }

        public UnityEvent OnDeath, OnReceiveDamage, OnBecomeVulnerable, OnResetDamage;

        [Tooltip("When this gameObject is damaged, these other gameObjects are notified.")]
        public List<MonoBehaviour> onDamageMessageReceivers;

        protected float _timeSinceLastHit = 0.0f;
        protected Collider _collider;

        System.Action schedule;

        // Start is called before the first frame update
        void Start()
        {
            ResetDamage();
            _collider = GetComponent<Collider>();
        }

        // Update is called once per frame
        void Update()
        {
            if (isInvulnerable)
            {
                _timeSinceLastHit += Time.deltaTime;
                if (_timeSinceLastHit > invulnerabilityTime)
                {
                    _timeSinceLastHit = 0.0f;
                    isInvulnerable = false;
                    OnBecomeVulnerable.Invoke();
                }
            }
        }

        void LateUpdate()
        {
            if (schedule != null)
            {
                schedule();
                schedule = null;
            }
        }

        public void ResetDamage()
        {
            currentHitPoints = maxHitPoints;
            currentArmor = maxArmor;
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
            if (currentHitPoints <= 0 || isInvulnerable)
                return;

            // isInvulnerable = true;  // enable this line if want to make player invul after getting hit.
            // currentHitPoints -= data.amount;
            CalculateDamageDone(data.amount);

            if (currentHitPoints <= 0)
                schedule += OnDeath.Invoke;
            else OnReceiveDamage.Invoke();

            var messageType = currentHitPoints <= 0 ? MessageType.DEAD : MessageType.DAMAGED;

            for (var i = 0; i < onDamageMessageReceivers.Count; ++i)
            {
                var receiver = onDamageMessageReceivers[i] as IMessageReceiver;
                receiver.OnReceiveMessage(messageType, this, data);
            }
        }

        void CalculateDamageDone(float amount)
        {
            currentArmor -= amount;
            if (currentArmor < 0) // when the dmg is greater than current armor
            {
                amount = Mathf.Abs(currentArmor); // save the leftover dmg
                currentArmor = 0;
                currentHitPoints -= amount;
            }
        }
    }

}