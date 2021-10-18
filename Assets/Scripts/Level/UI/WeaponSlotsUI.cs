using UnityEngine;

namespace MainGame
{
    public class WeaponSlotsUI : MonoBehaviour
    {
        float originPosY = 0;
        float posYChange = 25;

        private Weapon[] equippedWeapons = new Weapon[3];

        public void UpdateActiveSlotUI(PlayerWeapons playerWeapons)
        {
            int activeSlot = (int)playerWeapons.GetActiveWeapon().slot;

            foreach (RectTransform rect in transform)
            {
                if (rect.GetSiblingIndex() == activeSlot)
                    LeanTween.moveY(rect, posYChange, 0.25f).setOnComplete(() => rect.GetComponent<CanvasGroup>().alpha = 0.9f);
                else LeanTween.moveY(rect, originPosY, 0.25f).setOnComplete(() => rect.GetComponent<CanvasGroup>().alpha = 0.65f);
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
                    LeanTween.moveY(rect, posYChange, 0.25f).setOnComplete(() => rect.GetComponent<CanvasGroup>().alpha = 0.9f);
                else LeanTween.moveY(rect, originPosY, 0.25f).setOnComplete(() => rect.GetComponent<CanvasGroup>().alpha = 0.65f);
            }

        }
    }
}