using System.Collections;
using UnityEngine;
using System.Linq;

namespace MainGame
{
    public partial class Weapon : MonoBehaviour
    {
        [Header("Information")]
        public PlayerWeapons.WeaponSlot slot;
        public string weaponName;
        public Rarity rarity = Rarity.Normal;
        [SerializeField] private int ammoCount;
        [SerializeField] private int clipSize = 10;
        [Range(1, 10)]
        [SerializeField] private float fireRate = 5;
        [Range(0.5f, 3)]
        [SerializeField] private float reloadTime = 1f;
        public float ReloadTime { get { return reloadTime; } }
        private float damage = 10f;
        public float Damage { get { return damage; } }

        public bool IsReloading { get; private set; }
        public bool IsHolstered { get; private set; }
        public bool IsFiring { get; private set; }

        [Header("References")]
        public ParticleSystem bulletParticle;

        private float accumulatedTime = 0;

        // Start is called before the first frame update
        void Start()
        {
            FillAmmo();
        }

        private void OnEnable()
        {
            transform.GetComponent<SpriteRenderer>().color = CommonClass.RarityColor.ElementAtOrDefault((int)rarity).Value;
        }

        public void StartFiring()
        {
            IsFiring = true;
            accumulatedTime = 0;
        }

        public void UpdateFiring(float deltaTime)
        {
            accumulatedTime += deltaTime;
            float fireInterval = 1f / fireRate;
            while (accumulatedTime >= 0f)
            {
                FireBullets();
                accumulatedTime -= fireInterval;
            }
        }

        void FireBullets()
        {
            // Ammo
            if (ammoCount <= 0)
                return;
            ammoCount--;

            // Fire
            AdjustParticleRotation(bulletParticle);
            bulletParticle.Play();
            //bulletParticle.Emit(1);


            //ray.origin = raycastOrigin.position;
            //ray.direction = raycastDestination.position - raycastOrigin.position;

            //var tracer = Instantiate(tracerEffect, ray.origin, Quaternion.identity);
            //tracer.AddPosition(ray.origin);

            //if (Physics.Raycast(ray, out hitInfo))
            //{
            //    hitEffect.transform.position = hitInfo.point;
            //    hitEffect.transform.forward = hitInfo.normal;
            //    hitEffect.Emit(1);

            //    // Collision impulse
            //    var rb = hitInfo.collider.GetComponent<Rigidbody>();
            //    if (rb)
            //        rb.AddForceAtPosition(ray.direction * 20, hitInfo.point, ForceMode.Impulse);

            //    tracer.transform.position = hitInfo.point;
            //}

            //recoil.GenerateRecoil(weaponName);
        }

        void AdjustParticleRotation(ParticleSystem part)
        {
            var mainPartPS = part.main;
            mainPartPS.startRotation = transform.rotation.eulerAngles.z * -1 * Mathf.Deg2Rad;
            foreach (Transform t in part.transform)
            {
                ParticleSystem ps = t.GetComponent<ParticleSystem>();
                if (ps)
                {
                    var mainPS = ps.main;
                    mainPS.startRotation = mainPartPS.startRotation;
                }
            }
        }

        public void StopFiring()
        {
            IsFiring = false;
        }

        public bool CheckOutOfAmmo()
        {
            return ammoCount <= 0;
        }

        public IEnumerator Reload()
        {
            if (ammoCount == clipSize)
                yield return null;

            IsReloading = true;

            float elapsedTime = 0;
            while (elapsedTime <= reloadTime)
            {
                // TODO UI reload
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            FillAmmo();
            IsReloading = false;
        }

        public void FillAmmo()
        {
            ammoCount = clipSize;
        }

        public void SetWeaponHolstered(bool cd)
        {
            IsHolstered = cd;
        }

        public void ChangeColorByRarity(int targetRarity)
        {
            transform.GetComponent<SpriteRenderer>().color = CommonClass.RarityColor.ElementAtOrDefault(targetRarity).Value;
        }
    }
}