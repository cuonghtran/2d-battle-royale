using UnityEngine;
using System;

namespace MainGame
{
    public class UtilsClass
    {
        public static Vector3 GetMouseWorldPosition(Camera worldCamera)
        {
            Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
            vec.z = 0f;
            return vec;
        }

        public static Vector3 GetMouseWorldPositionWithZ()
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        }

        public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
        }

        public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
        {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }

        public static Color GetColorFromString(string color)
        {
            float red = Hex_to_Dec(color.Substring(0, 2));
            float green = Hex_to_Dec(color.Substring(2, 2));
            float blue = Hex_to_Dec(color.Substring(4, 2));
            float alpha = 1f;
            if (color.Length >= 8)
            {
                // Color string contains alpha
                alpha = Hex_to_Dec(color.Substring(6, 2));
            }
            return new Color(red, green, blue, alpha);
        }

        public static float Hex_to_Dec(string hex)
        {
            return Convert.ToInt32(hex, 16) / 255f;
        }
    }

}