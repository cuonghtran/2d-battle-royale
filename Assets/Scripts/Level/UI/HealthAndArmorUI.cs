using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MainGame
{
    public class HealthAndArmorUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _healthFillImage;
        [SerializeField] private Image _armorFillImage;
        public float updateSpeed = 0.18f;

        // Start is called before the first frame update
        void Start()
        {
            Reset();
        }

        void Reset()
        {
            _healthFillImage.fillAmount = 1;
            _armorFillImage.fillAmount = 1;
        }

        public void ChangeHitPointUI(Damageable damageable)
        {
            StartCoroutine(UpdateHpAndArmor(damageable));
        }

        IEnumerator UpdateHpAndArmor(Damageable damageable)
        {
            float armorPercentage = damageable.CurrentArmor / damageable.maxArmor;
            float hpPercentage = damageable.CurrentHitPoints / damageable.maxHitPoints;

            if (armorPercentage > 0)
                yield return StartCoroutine(UpdateArmorSequence(armorPercentage));
            StartCoroutine(UpdateHitPointSequence(hpPercentage));
        }

        IEnumerator UpdateArmorSequence(float pct)
        {
            float preChangePct = _armorFillImage.fillAmount;
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