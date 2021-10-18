using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MainGame
{
    public class HealthAndArmorUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Image healthFillImage;
        [SerializeField] Image armorFillImage;
        public float updateSpeed = 0.18f;

        // Start is called before the first frame update
        void Start()
        {
            Reset();
        }

        void Reset()
        {
            healthFillImage.fillAmount = 1;
            armorFillImage.fillAmount = 1;
        }

        public void ChangeHitPointUI(Damageable damageable)
        {
            StartCoroutine(UpdateHpAndArmor(damageable));
        }

        IEnumerator UpdateHpAndArmor(Damageable damageable)
        {
            float armorPercentage = damageable.currentArmor / damageable.maxArmor;
            float hpPercentage = damageable.currentHitPoints / damageable.maxHitPoints;

            yield return StartCoroutine(UpdateArmorSequence(armorPercentage));
            StartCoroutine(UpdateHitPointSequence(hpPercentage));
        }

        IEnumerator UpdateArmorSequence(float pct)
        {
            float preChangePct = armorFillImage.fillAmount;
            float elapsed = 0;

            while (elapsed < updateSpeed)
            {
                elapsed += Time.deltaTime;
                armorFillImage.fillAmount = Mathf.Lerp(preChangePct, pct, elapsed / updateSpeed);
                yield return null;
            }
            armorFillImage.fillAmount = pct;
        }

        IEnumerator UpdateHitPointSequence(float pct)
        {
            float preChangePct = healthFillImage.fillAmount;
            float elapsed = 0;

            while (elapsed < updateSpeed)
            {
                elapsed += Time.deltaTime;
                healthFillImage.fillAmount = Mathf.Lerp(preChangePct, pct, elapsed / updateSpeed);
                yield return null;
            }
            healthFillImage.fillAmount = pct;
        }
    }
}