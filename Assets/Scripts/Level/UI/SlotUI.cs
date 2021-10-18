using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace MainGame
{
    public class SlotUI : MonoBehaviour
    {
        [SerializeField] private Image weaponImage;

        public void UpdateSlotImage(Sprite sprite, int rarity)
        {
            weaponImage.sprite = sprite;
            weaponImage.color = CommonClass.RarityColor.ElementAtOrDefault(rarity).Value;
            weaponImage.gameObject.SetActive(true);
        }
    }
}