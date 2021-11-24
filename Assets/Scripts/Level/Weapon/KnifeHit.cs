using UnityEngine;
using Mirror;

namespace MainGame
{
    public class KnifeHit : NetworkBehaviour
    {
        [SerializeField] LayerMask attacksWhat;
        private Weapon _knifeWeapon;
        private Damageable _ownerDamageable;
        private string _playerOwner;
        private float _damageAmount = 25f;

        private void Start()
        {
            _knifeWeapon = transform.GetComponentInParent<Weapon>();
            _ownerDamageable = transform.GetComponentInParent<Damageable>();

            _playerOwner = _knifeWeapon.playerOwner;
        }

        [Server]
        private void OnTriggerEnter2D(Collider2D collision)
        {
            var dmg = collision.GetComponent<Damageable>();
            if (dmg == null || _ownerDamageable == dmg)
                return;
            SendDamageMessage(collision.gameObject);
        }

        void SendDamageMessage(GameObject other)
        {
            var d = other.GetComponent<Damageable>();
            if (d == null)
                return;

            var msg = new Damageable.DamageMessage()
            {
                damager = this.gameObject,
                sourcePlayer = _playerOwner,
                amount = _damageAmount,
                direction = other.transform.position - transform.position,
            };

            d.ApplyDamage(msg);
        }
    }
}