using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MainGame
{
    public class BulletParticle : NetworkBehaviour
    {
        [Header("Bullet Properties")]
        [Range(15, 30)]
        [SerializeField] private float speed = 15f;
        [Range(0.5f, 2.5f)]
        [SerializeField] private float lifeTime = 1f;
        [Range(0.1f, 1f)]
        [SerializeField] private float size = 0.2f;
        [Range(0.5f, 25f)]
        [SerializeField] private float spreadAngle = 1f;
        [Range(5f, 40f)]
        [SerializeField] private float damageAmount = 5f;

        [SerializeField] private bool isBurstWeapon;

        [SerializeField] Weapon bulletForWeapon;

        private ParticleSystem _particleSystem;
        private List<ParticleCollisionEvent> collisionEvents;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();

            InitParticleData();
        }

        void Start()
        {
            collisionEvents = new List<ParticleCollisionEvent>();
        }

        void InitParticleData()
        {
            // main particle
            var mainPS = _particleSystem.main;
            var shapePS = _particleSystem.shape;

            mainPS.startSpeed = isBurstWeapon ? new ParticleSystem.MinMaxCurve(speed - 3, speed + 3) : speed;
            mainPS.startLifetime = lifeTime;
            mainPS.startSize = size;
            mainPS.startRotation = transform.parent.rotation.eulerAngles.z * -1 * Mathf.Deg2Rad;
            shapePS.angle = spreadAngle;
        }

        [Server]
        void OnParticleCollision(GameObject other)
        {
            int numCollisionEvents = _particleSystem.GetCollisionEvents(other, collisionEvents);

            for (int i = 0; i < numCollisionEvents; i++)
            {
                // Collision impulse
                var rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddForceAtPosition(other.transform.position - transform.position * 5, transform.position, ForceMode.Impulse);

                SendDamageMessage(other);
            }
        }

        void SendDamageMessage(GameObject other)
        {
            var d = other.GetComponent<Damageable>();
            if (d == null)
                return;

            var msg = new Damageable.DamageMessage()
            {
                damager = this.gameObject,
                amount = damageAmount,
                direction = other.transform.position - transform.position,
            };

            d.ApplyDamage(msg);
        }
    }
}