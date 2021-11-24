using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

namespace MainGame
{
    public class PlayerAvatarOverheadUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private ClientPlayerAvatarRuntimeCollection _playerAvatars;

        [Header("References")]
        public float updateSpeed = 0.18f;
        [SerializeField] private PlayerNetwork _playerNetwork;
        [SerializeField] private Damageable _damageable;
        [SerializeField] private Image _healthFillImage;
        [SerializeField] private Image _armorFillImage;
        [SerializeField] private TMP_Text _playerName_Text;


        private void OnEnable()
        {
            _playerAvatars.ItemAdded += PlayerAvatarAdded;
            _damageable.OnHealthChanged += HandleHealthChanged;

            _playerName_Text.text = NetworkGamePlayer.singleton.GetDisplayName();
        }

        private void OnDisable()
        {
            _playerAvatars.ItemAdded -= PlayerAvatarAdded;
            _damageable.OnHealthChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(Damageable damageable)
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

        private void PlayerAvatarAdded(ClientPlayerAvatar clientPlayerAvatar)
        {
            if (clientPlayerAvatar.hasAuthority)
            {
                // Set display name
                //_playerName_Text.text = GetPlayerName(connectionToClient.connectionId);
            }
        }
    }
}
