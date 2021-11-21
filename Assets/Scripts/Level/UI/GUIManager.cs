using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;
using Mirror;

namespace MainGame
{
    public class GUIManager : MonoBehaviour
    {
        public static GUIManager Instance;

        [Header("Data")]
        [SerializeField] private ClientPlayerAvatarRuntimeCollection _playerAvatars;

        [Header("References")]
        [SerializeField] private Image _healthFillImage;
        [SerializeField] private Image _armorFillImage;
        public float updateSpeed = 0.18f;
        [SerializeField] private TMP_Text _playerName_Text;
        [SerializeField] private TMP_Text _ammoMag_Text;

        private Damageable _clientDamageable;

        #region Setup

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            _playerAvatars.ItemAdded += PlayerAvatarAdded;
            _playerAvatars.ItemRemoved += PlayerAvatarRemoved;
        }

        private void OnEnable()
        {
            PlayerWeapons.OnAmmoChanged += UpdateAmmoUI;
        }

        private void OnDisable()
        {
            _playerAvatars.ItemAdded -= PlayerAvatarAdded;
            _playerAvatars.ItemRemoved -= PlayerAvatarRemoved;
            PlayerWeapons.OnAmmoChanged -= UpdateAmmoUI;
        }

        private void PlayerAvatarAdded(ClientPlayerAvatar clientPlayerAvatar)
        {
            if (clientPlayerAvatar.hasAuthority)
            {
                // Set damageable
                var damageableExists = clientPlayerAvatar.TryGetComponent(out Damageable localDamageable);

                Assert.IsTrue(damageableExists,
                "Damageable component not found on ClientPlayerAvatar");
                
                // _playerName_Text.text = GetPlayerName(netIdentity.connectionToClient.connectionId);

                _clientDamageable = localDamageable;
                _clientDamageable.OnHealthChanged += HandleHealthChanged;
            }
            else Debug.Log("no auth");
        }

        private void PlayerAvatarRemoved(ClientPlayerAvatar clientPlayerAvatar)
        {
            _clientDamageable.OnHealthChanged -= HandleHealthChanged;
        }

        private string GetPlayerName(int connectionId)
        {
            var playerData = MainGameNetworkManager.GetPlayerData(connectionId);
            if (playerData.HasValue)
                return playerData.Value.PlayerName;
            return "Anonymous";
        }

        #endregion

        #region Ammo

        public void UpdateAmmoUI(int ammo)
        {
            _ammoMag_Text.text = ammo.ToString();
        }

        #endregion

        #region Health and Armor

        public void HandleHealthChanged(Damageable damageable)
        {
            StartCoroutine(UpdateHpAndArmor(damageable));
        }

        IEnumerator UpdateHpAndArmor(Damageable damageable)
        {
            float hpPercentage = damageable.CurrentHitPoints / damageable.maxHitPoints;
            float armorPercentage = damageable.CurrentArmor / damageable.maxArmor;

            yield return StartCoroutine(UpdateArmorSequence(armorPercentage));
            StartCoroutine(UpdateHitPointSequence(hpPercentage));
        }

        IEnumerator UpdateArmorSequence(float pct)
        {
            float preChangePct = _armorFillImage.fillAmount;
            if (preChangePct == pct) yield break;

            float elapsed = 0;
            while (elapsed < updateSpeed)
            {
                elapsed += Time.deltaTime;
                _armorFillImage.fillAmount = Mathf.Lerp(preChangePct, pct, elapsed / updateSpeed);
                yield return null;
            }
            _armorFillImage.fillAmount = pct;
        }

        IEnumerator UpdateHitPointSequence(float pct)
        {
            float preChangePct = _healthFillImage.fillAmount;
            if (preChangePct == pct) yield break;

            float elapsed = 0;
            while (elapsed < updateSpeed)
            {
                elapsed += Time.deltaTime;
                _healthFillImage.fillAmount = Mathf.Lerp(preChangePct, pct, elapsed / updateSpeed);
                yield return null;
            }
            _healthFillImage.fillAmount = pct;
        }

        #endregion
    }
}
