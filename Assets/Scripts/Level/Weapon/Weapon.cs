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
        public int AmmoCount { get { return ammoCount; } }
        [SerializeField] private int clipSize = 10;
        [Range(1, 10)]
        [SerializeField] private float fireRate = 5;
        [Range(0.5f, 3)]
        [SerializeField] private float reloadTime = 1f;
        public float ReloadTime { get { return reloadTime; } }
        private float damage = 10f;
        public float Damage { get { return damage; } }
        public float WeaponCooldown { get { return 1f / fireRate; } }

        public bool IsReloading { get; private set; }
        public bool IsHolstered { get; private set; }
        public bool IsFiring { get; private set; }

        [Header("Particles")]
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

        public void ReduceAmmo()
        {
            // Ammo
            if (ammoCount <= 0)
                return;
            ammoCount--;
        }

        public void OnFireBullets()
        {
            // Fire
            AdjustParticleRotation(bulletParticle);
            bulletParticle.Play();
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

        public void ChangeColorByRarity(int targetRarity)
        {
            transform.GetComponent<SpriteRenderer>().color = CommonClass.RarityColor.ElementAtOrDefault(targetRarity).Value;
        }
    }
}