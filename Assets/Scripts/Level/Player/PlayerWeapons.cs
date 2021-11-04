using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

namespace MainGame
{
    public class PlayerWeapons : NetworkBehaviour
    {
        public enum WeaponSlot
        {
            First = 0,
            Second = 1,
            Third = 2
        }
        public Transform[] weaponSlots;

        public Weapon[] equippedWeapons = new Weapon[3];
        [SerializeField] private Weapon activeWeapon;
        //[SyncVar(hook = nameof(OnActiveWeaponChanged))]
        [SyncVar]
        [SerializeField] private int activeWeaponIndex = -1;

        public Animator knifeAnimator;

        private bool onCooldown;
        private string _knifeWeaponName = "Knife";
        private float _knifeCooldown = 0.5f;
        private int _hashKnifeAttack = Animator.StringToHash("Knife_Attack");

        public UnityEvent OnChangeWeapon, OnPickupWeapon, OnReload;


        public override void OnStartAuthority()
        {
            enabled = true;

            //if (activeWeapon && activeWeaponIndex != -1)
            //{
            //    CmdChangeActiveWeapon(activeWeaponIndex);
            //    OnPickupWeapon.Invoke();
            //}
        }

        [ClientCallback]
        void Update()
        {
            //if (!hasAuthority || !isLocalPlayer) return;

            ChangeWeapon();
            StartCoroutine(ReloadActiveWeapon());
        }

        void ChangeWeapon()
        {
            if (!activeWeapon.IsReloading)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    CmdChangeActiveWeapon((int)WeaponSlot.First);

                if (Input.GetKeyDown(KeyCode.Alpha2))
                    CmdChangeActiveWeapon((int)WeaponSlot.Second);

                if (Input.GetKeyDown(KeyCode.Alpha3))
                    CmdChangeActiveWeapon((int)WeaponSlot.Third);
            }
        }

        void OnActiveWeaponChanged(int oldWeaponIndex, int newWeaponIndex)
        {
            RpcChangeWeapon(newWeaponIndex);
        }

        [Command]
        public void CmdChangeActiveWeapon(int newWeaponIndex)
        {
            activeWeaponIndex = newWeaponIndex;
            RpcChangeWeapon(newWeaponIndex);
        }

        [ClientRpc]
        void RpcChangeWeapon(int newWeaponIndex)
        {
            OnChangeWeapon.Invoke();
            StartCoroutine(SwitchWeapon(newWeaponIndex));
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

        IEnumerator ReloadActiveWeapon()
        {
            if (activeWeapon && (Input.GetKeyDown(KeyCode.R) || activeWeapon.CheckOutOfAmmo()))
            {
                if (activeWeapon.slot != WeaponSlot.Third) // don't reload third weapons
                {
                    activeWeapon.StopFiring();
                    OnReload.Invoke();
                    yield return StartCoroutine(activeWeapon.Reload());
                    // GUIManager.Instance.UpdateAmmoUI(activeWeapon.AmmoCount);
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

        public void Pickup(Weapon newWeapon)
        {
            int newWeaponIndex = (int)newWeapon.slot;
            //var weapon = GetWeaponByIndex(newWeaponIndex);
            //if (weapon)
            //    return; // do nothing if pickup an already owned weapon

            //weapon = newWeapon;
            equippedWeapons[newWeaponIndex] = newWeapon;
            newWeapon.transform.SetParent(weaponSlots[newWeaponIndex], false);
            //newWeapon.ChangeColorByRarity((int)newWeapon.rarity);

            CmdChangeActiveWeapon(newWeaponIndex);
            OnPickupWeapon.Invoke();
        }

        IEnumerator HolsterCurrentWeapon()
        {
            onCooldown = true;

            foreach(Weapon wp in equippedWeapons)
            {
                if (wp != null) wp.gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(0.1f);

            //var weapon = GetWeaponByIndex(index);
            //if (weapon)
            //{
            //    weapon.gameObject.SetActive(false);
            //    yield return new WaitForSeconds(0.1f);
            //}
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
                //GUIManager.Instance.UpdateAmmoUI(weapon.AmmoCount);
            }
        }

        IEnumerator SwitchWeapon(int newWeaponIndex)
        {
            yield return StartCoroutine(HolsterCurrentWeapon());
            yield return StartCoroutine(ActivateWeapon(newWeaponIndex));
        }
    }
}