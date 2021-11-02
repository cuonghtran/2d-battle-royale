using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MainGame
{
    public class PlayerWeapons : MonoBehaviour
    {
        public enum WeaponSlot
        {
            First = 0,
            Second = 1,
            Third = 2
        }
        public Transform[] weaponSlots;
        public Weapon[] equippedWeapons = new Weapon[3];

        //[SyncVar(hook = nameof(OnActiveWeaponChanged))]
        [SerializeField] private Weapon activeWeapon;
        [SerializeField] private int activeWeaponIndex = -1;
        public Animator knifeAnimator;

        private bool onCooldown;
        private string _knifeWeaponName = "Knife";
        private float _knifeCooldown = 0.5f;
        private int _hashKnifeAttack = Animator.StringToHash("Knife_Attack");

        public UnityEvent OnChangeWeapon, OnPickupWeapon, OnReload;

        // Start is called before the first frame update
        void Start()
        {
            if (activeWeapon)
            {
                Equip(activeWeapon, false);
                OnPickupWeapon.Invoke();
            }
        }

        // Update is called once per frame
        void Update()
        {
            ChangeWeapon();
            StartCoroutine(ReloadActiveWeapon());
        }

        void ChangeWeapon()
        {
            if (!activeWeapon.IsReloading)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    SetActiveWeapon(WeaponSlot.First, false);

                if (Input.GetKeyDown(KeyCode.Alpha2))
                    SetActiveWeapon(WeaponSlot.Second, false);

                if (Input.GetKeyDown(KeyCode.Alpha3))
                    SetActiveWeapon(WeaponSlot.Third, false);
            }
        }

        void OnActiveWeaponChanged(Weapon _old, Weapon _new)
        {
            Equip(activeWeapon, false);
            OnPickupWeapon.Invoke();
        }

        IEnumerator ReloadActiveWeapon()
        {
            if (activeWeapon && (Input.GetKeyDown(KeyCode.R) || activeWeapon.CheckOutOfAmmo()))
            {
                if (activeWeapon.slot != WeaponSlot.Third) // don't reload third weapons
                {
                    activeWeapon.StopFiring();
                    OnReload.Invoke();
                    yield return StartCoroutine(activeWeapon.Reload());
                    GUIManager.Instance.UpdateAmmoUI(activeWeapon.AmmoCount);
                }
            }
        }

        void LateUpdate()
        {
            if (activeWeapon)
            {
                if (!onCooldown && !activeWeapon.IsReloading)
                {
                    // if active weapon is a knife
                    if (activeWeapon.slot == WeaponSlot.Third && activeWeapon.name == _knifeWeaponName)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            knifeAnimator.SetTrigger(_hashKnifeAttack);
                            StartCoroutine(ReloadKnife());
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            activeWeapon.StartFiring();
                        }

                        if (activeWeapon.IsFiring)
                        {
                            activeWeapon.UpdateFiring(Time.deltaTime);
                            GUIManager.Instance.UpdateAmmoUI(activeWeapon.AmmoCount);
                        }

                        if (Input.GetMouseButtonUp(0))
                        {
                            activeWeapon.StopFiring();
                        }
                    }
                }
            }
        }

        IEnumerator ReloadKnife()
        {
            onCooldown = true;

            float elapsedTime = 0;
            while (elapsedTime <= _knifeCooldown)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            onCooldown = false;
        }

        Weapon GetWeaponByIndex(int index)
        {
            if (index < 0 || index >= equippedWeapons.Length)
                return null;
            return equippedWeapons[index];
        }

        public Weapon GetActiveWeapon()
        {
            return GetWeaponByIndex(activeWeaponIndex);
        }

        public void Equip(Weapon newWeapon, bool isPickup)
        {
            int weaponSlotIndex = (int)newWeapon.slot;
            var weapon = GetWeaponByIndex(weaponSlotIndex);
            if (weapon)
                return; // do nothing if pickup an already owned weapon

            weapon = newWeapon;
            weapon.transform.SetParent(weaponSlots[weaponSlotIndex], false);
            weapon.ChangeColorByRarity((int)newWeapon.rarity);
            equippedWeapons[weaponSlotIndex] = weapon;

            SetActiveWeapon(newWeapon.slot, isPickup);
        }

        void SetActiveWeapon(WeaponSlot weaponSlot, bool isPickup)
        {
            if (equippedWeapons[(int)weaponSlot] == null)
                return;

            int holsterIndex = activeWeaponIndex;
            int activateIndex = (int)weaponSlot;
            if (holsterIndex != activateIndex) // only switch different weapons
            {
                StartCoroutine(SwitchWeapon(holsterIndex, activateIndex, isPickup));
            }
        }

        IEnumerator HolsterWeapon(int index)
        {
            onCooldown = true;
            var weapon = GetWeaponByIndex(index);
            if (weapon)
            {
                weapon.SetWeaponHolstered(true);
                weapon.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.1f);
            }
        }

        IEnumerator ActivateWeapon(int index)
        {
            var weapon = GetWeaponByIndex(index);
            if (weapon)
            {
                activeWeapon = weapon;
                weapon.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.05f);
                onCooldown = false;
                weapon.SetWeaponHolstered(false);
                GUIManager.Instance.UpdateAmmoUI(weapon.AmmoCount);
            }
        }

        IEnumerator SwitchWeapon(int currentIndex, int activateIndex, bool isPickup)
        {
            yield return StartCoroutine(HolsterWeapon(currentIndex));
            yield return StartCoroutine(ActivateWeapon(activateIndex));
            activeWeaponIndex = activateIndex;
            if (isPickup)
                OnPickupWeapon.Invoke();
            else OnChangeWeapon.Invoke();
        }
    }
}