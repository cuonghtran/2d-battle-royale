using UnityEngine;

namespace MainGame
{
    public class WeaponSlotsUI : MonoBehaviour
    {
        float _transitionTime = 0.15f;
        float _activateAlpha = 0.85f;
        float _deactivateAlpha = 0.6f;
        float _originPosY = 0;
        float _posYChange = 25;

        private Weapon[] equippedWeapons = new Weapon[3];

        private void OnEnable()
        {
            PlayerWeapons.OnWeaponChanged += UpdateActiveSlotUI;
            PlayerWeapons.OnWeaponPickedUp += UpdateActiveSlotWhenPickupUI;
        }

        private void OnDisable()
        {
            PlayerWeapons.OnWeaponChanged -= UpdateActiveSlotUI;
            PlayerWeapons.OnWeaponPickedUp -= UpdateActiveSlotWhenPickupUI;
        }

        public void UpdateActiveSlotUI(PlayerWeapons playerWeapons)
        {
            int activeSlot = (int)playerWeapons.GetActiveWeapon().slot;

            foreach (RectTransform rect in transform)
            {
                if (rect.GetSiblingIndex() == activeSlot)
                    MoveSlot(rect, _posYChange, _transitionTime, _activateAlpha);
                else MoveSlot(rect, _originPosY, _transitionTime, _deactivateAlpha);
            }
        }

        public void UpdateActiveSlotWhenPickupUI(PlayerWeapons playerWeapons)
        {
            equippedWeapons = playerWeapons.equippedWeapons;
            int activeSlot = (int)playerWeapons.GetActiveWeapon().slot;

            foreach (RectTransform rect in transform)
            {
                int wpIndex = rect.GetSiblingIndex();
                // update equipped weapons
                if (equippedWeapons[wpIndex] != null)
                {
                    var sprite = equippedWeapons[wpIndex].GetComponent<SpriteRenderer>().sprite;
                    rect.GetComponent<SlotUI>().UpdateSlotImage(sprite, (int)equippedWeapons[wpIndex].rarity);
                }

                if (wpIndex == activeSlot)
                    MoveSlot(rect, _posYChange, _transitionTime, _activateAlpha);
                else MoveSlot(rect, _originPosY, _transitionTime, _deactivateAlpha);
            }

        }

        private void MoveSlot(RectTransform rect, float yPositionChange, float transitionTime, float alphaValue)
        {
            LeanTween.moveY(rect, yPositionChange, transitionTime).setOnComplete(() => rect.GetComponent<CanvasGroup>().alpha = alphaValue);
        }
    }
}