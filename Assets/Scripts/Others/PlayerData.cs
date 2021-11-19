using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    public struct PlayerData
    {
        public string PlayerName { get; set; }

        public PlayerData(string playerName)
        {
            PlayerName = playerName;
        }
    }
}
