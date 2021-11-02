using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    public class CommonClass
    {
        public static Dictionary<string, Color> RarityColor = new Dictionary<string, Color>()
        {
            { "Normal", new Color32(135, 135, 135, 255) },
            { "Rare", new Color32(20, 150, 225, 255) },
            { "Epic", new Color32(165, 20, 225, 255) },
            { "Legendary", new Color32(225, 165, 20, 255) },
            { "Blank", new Color32(255, 255, 255, 255) },
        };
    }

}
