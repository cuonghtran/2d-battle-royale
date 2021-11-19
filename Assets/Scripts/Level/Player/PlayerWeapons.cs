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

        [SyncVar]
        [SerializeField] private int activeWeaponIndex = -1;

        public Animator knifeAnimator;

        private bool _onCooldown;
        private string _knifeWeaponName = "Knife";
        private float _weaponCooldownTime = 0f;
        private float _knifeCooldown = 0.5f;
        private int _hashKnifeAttack = Animator.StringToHash("Knife_Attack");
        private Weapon weaponToBePickedUp = null;

        public UnityEvent OnChangeWeapon, OnReload;


        //public override void OnStartAuthority()
        //{
        //    enabled = true;
        //}

        
        void Update()
        {
            ChangeWeapon();
            StartCoroutine(ReloadActiveWeapon());
            WeaponFireHandler();
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

        [Command]
        public void CmdChangeActiveWeapon(int newWeaponIndex)
        {
            if (activeWeaponIndex == newWeaponIndex || equippedWeapons[newWeaponIndex] == null)
                return;

            activeWeaponIndex = newWeaponIndex;
            _weaponCooldownTime = 0;
            RpcChangeWeapon(newWeaponIndex);
        }

        [ClientRpc]
        private void RpcChangeWeapon(int newWeaponIndex)
        {
            OnChangeWeapon.Invoke();
            StartCoroutine(SwitchWeapon(newWeaponIndex));
        }

        private void WeaponFireHandler()
        {
            if (activeWeapon && !_onCooldown && !activeWeapon.IsReloading)
            {
                if (Input.GetMouseButton(0))
                {
                    if (activeWeapon.slot == WeaponSlot.Third && activeWeapon.name == _knifeWeaponName)
                    {
                        CmdUseKnife();
                    }
                    else
                    {
                        if (Time.time >= _weaponCooldownTime)
                        {
                            _weaponCooldownTime = Time.time + activeWeapon.WeaponCooldown;
                            CmdShoot();
                        }
                    }
                }
            }
        }

        [Command]
        private void CmdUseKnife()
        {
            RpcUseKnife();
        }

        [ClientRpc]
        private void RpcUseKnife()
        {
            knifeAnimator.SetTrigger(_hashKnifeAttack);
            StartCoroutine(ReloadKnife());
        }

        [Command]
        private void CmdShoot()
        {
            activeWeapon.FireBullets();
            RpcOnShoot();
        }

        [ClientRpc]
        private void RpcOnShoot()
        {
            activeWeapon.OnFireBullets();
        }

        IEnumerator ReloadActiveWeapon()
        {
            if (activeWeapon && (Input.GetKeyDown(KeyCode.R) || activeWeapon.CheckOutOfAmmo()))
            {
                if (activeWeapon.slot != WeaponSlot.Third) // don't reload third weapons
                {
                    OnReload.Invoke();
                    yield return StartCoroutine(activeWeapon.Reload());
                    // GUIManager.Instance.UpdateAmmoUI(activeWeapon.AmmoCount);
                }
            }
        }

        IEnumerator ReloadKnife()
        {
            _onCooldown = true;

            float elapsedTime = 0;
            while (elapsedTime <= _knifeCooldown)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _onCooldown = false;
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

        public void PostPickUpHandler(Weapon newWeapon)
        {
            int newWeaponIndex = (int)newWeapon.slot;
            equippedWeapons[newWeaponIndex] = newWeapon;
            newWeapon.transform.SetParent(weaponSlots[newWeaponIndex], false);

            CmdChangeActiveWeapon(newWeaponIndex);
            //OnPickupWeapon.Invoke();
        }

        IEnumerator HolsterCurrentWeapon()
        {
            _onCooldown = true;

            foreach(Weapon wp in equippedWeapons)
            {
                if (wp != null) wp.gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(0.1f);
        }

        IEnumerator ActivateWeapon(int index)
        {
            var weapon = GetWeaponByIndex(index);
            if (weapon)
            {
                activeWeapon = weapon;
                weapon.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.05f);
                _onCooldown = false;
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