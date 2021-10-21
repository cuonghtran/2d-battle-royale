using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MainGame
{
    public class GUIManager : MonoBehaviour
    {
        public static GUIManager Instance;

        public TMP_Text BigAmmoMag_Text;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void UpdateAmmoUI(int ammo)
        {
            BigAmmoMag_Text.text = ammo.ToString();
        }
    }
}
