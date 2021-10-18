using UnityEngine;

namespace MainGame
{
    public class LevelManager : MonoBehaviour
    {
        private static Camera _mainCamera;
        public static Camera MainCamera
        {
            get
            {
                if (!_mainCamera) _mainCamera = Camera.main;
                return _mainCamera;
            }
        }
    }
}