using MainGame.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    public class InputManager : MonoBehaviour
    {
        private static IDictionary<string, int> mapStates = new Dictionary<string, int>();

        private static Controls _controls;
        public static Controls _Controls
        {
            get
            {
                if (_controls != null) return _controls;
                return _controls = new Controls();
            }
        }

        private void Awake()
        {
            if (_controls != null) return;
            _controls = new Controls();
        }

        private void OnEnable()
        {
            _Controls.Enable();
        }

        private void OnDisable()
        {
            _Controls.Disable();
        }

        private void OnDestroy()
        {
            _controls = null;
        }

        public static void AddBlock(string mapName)
        {
            mapStates.TryGetValue(mapName, out int value);
            mapStates[mapName] = value + 1;
            UpdateMapState(mapName);
        }

        public static void RemoveBlock(string mapName)
        {
            mapStates.TryGetValue(mapName, out int value);
            mapStates[mapName] = Mathf.Max(value - 1, 0);
            UpdateMapState(mapName);
        }

        private static void UpdateMapState(string mapName)
        {
            int value = mapStates[mapName];

            if (value > 0)
            {
                _Controls.asset.FindActionMap(mapName).Disable();
                return;
            }
            _Controls.asset.FindActionMap(mapName).Enable();
        }
    }
}
