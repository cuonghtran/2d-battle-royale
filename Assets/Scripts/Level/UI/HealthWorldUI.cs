using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MainGame
{
    public class HealthWorldUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Damageable _damageable;
        [SerializeField] private Image _healthFillImage;
        [SerializeField] private Image _armorFillImage;
        public float updateSpeed = 0.18f;

        private void OnEnable()
        {
            _damageable.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
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
    }
}
