using System;
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
        private PlayerNetwork _playerNetwork;

        private bool _onCooldown;
        private string _knifeWeaponName = "Knife";
        private float _weaponCooldownTime = 0f;
        private float _knifeCooldown = 0.7f;
        private int _hashKnifeAttack = Animator.StringToHash("Knife_Attack");
        private string _playerOwner;
        public Weapon weaponToBePickedUp;

        public UnityEvent OnReload;
        public static Action<int> OnAmmoChanged;
        public static Action<PlayerWeapons> OnWeaponChanged, OnWeaponPickedUp;


        public override void OnStartAuthority()
        {
            enabled = true;
            _playerOwner = NetworkGamePlayer.singleton.GetDisplayName();

            if (activeWeapon)
            {
                activeWeapon.playerOwner = _playerOwner;
                OnWeaponPickedUp.Invoke(this);
            }
        }

        void Update()
        {
            if (!isLocalPlayer) return;

            ChangeWeapon();
            StartCoroutine(ReloadActiveWeapon());
            WeaponFireHandler();
        }

        void ChangeWeapon()
        {
            if (!activeWeapon.IsReloading)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    CmdChangeActiveWeapon((int)WeaponSlot.First);
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    CmdChangeActiveWeapon((int)WeaponSlot.Second);
                }

                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    CmdChangeActiveWeapon((int)WeaponSlot.Third);
                }
            }
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
                            activeWeapon.ReduceAmmo();
                            UpdateAmmoUI();
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
                    UpdateAmmoUI();
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

        [Client]
        void UpdateAmmoUI()
        {
            if (!isLocalPlayer) return;
            OnAmmoChanged?.Invoke(activeWeapon.AmmoCount);
        }

        [Client]
        void UpdateWeaponSlotUIWhenChange()
        {
            if (!isLocalPlayer) return;
            OnWeaponChanged.Invoke(this);
        }

        [Client]
        void UpdateWeaponSlotUIWhenPickUp()
        {
            if (!isLocalPlayer) return;
            OnWeaponPickedUp.Invoke(this);
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
            StartCoroutine(SwitchWeapon(newWeaponIndex));
        }

        [Command]
        public void CmdActivatePickedUpWeapon(int newWeaponIndex)
        {
            if (activeWeaponIndex == newWeaponIndex || equippedWeapons[newWeaponIndex] == null)
                return;

            activeWeaponIndex = newWeaponIndex;
            _weaponCooldownTime = 0;
            RpcActivatePickedUpWeapon(newWeaponIndex);
        }

        [ClientRpc]
        private void RpcActivatePickedUpWeapon(int newWeaponIndex)
        {
            StartCoroutine(SwitchWeapon(newWeaponIndex, true));
        }

        [Client]
        public void PostPickUpHandler(Weapon newWeapon)
        {
            int newWeaponIndex = (int)newWeapon.slot;
            equippedWeapons[newWeaponIndex] = newWeapon;
            newWeapon.transform.SetParent(weaponSlots[newWeaponIndex], false);
            newWeapon.playerOwner = _playerOwner;
            newWeapon.SetUpOwner();

            CmdActivatePickedUpWeapon(newWeaponIndex);
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

        IEnumerator ActivateWeapon(int index, bool fromPickUp)
        {
            var weapon = GetWeaponByIndex(index);
            if (weapon)
            {
                activeWeapon = weapon;
                weapon.gameObject.SetActive(true);
                // update weapons UI
                if (fromPickUp)
                    UpdateWeaponSlotUIWhenPickUp();
                else UpdateWeaponSlotUIWhenChange();

                yield return new WaitForSeconds(0.05f);
                UpdateAmmoUI();
                _onCooldown = false;
            }
        }

        IEnumerator SwitchWeapon(int newWeaponIndex, bool fromPickUp = false)
        {
            yield return StartCoroutine(HolsterCurrentWeapon());
            yield return StartCoroutine(ActivateWeapon(newWeaponIndex, fromPickUp));
        }
    }
}