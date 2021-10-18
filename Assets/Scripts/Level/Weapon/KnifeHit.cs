using UnityEngine;

namespace MainGame
{
    public class KnifeHit : MonoBehaviour
    {
        [SerializeField] private Weapon _representWeapon;
        [SerializeField] LayerMask attacksWhat;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            //var d = collision.GetComponent<Damageable>();
            //if (d == null)
            //    return;

            //var msg = new Damageable.DamageMessage()
            //{
            //    damager = this,
            //    amount = _representWeapon.Damage,
            //    direction = collision.transform.position - transform.position,
            //    stopCamera = false
            //};

            //d.ApplyDamage(msg);
            //Debug.Log("collide");
        }
    }
}