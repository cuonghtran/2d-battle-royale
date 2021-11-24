using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

namespace MainGame
{
    public class PlayerNetwork : NetworkBehaviour
    {
        public string playerName;

        public override void OnStartAuthority()
        {
            enabled = true;
        }

        public override void OnStartLocalPlayer()
        {
            playerName = NetworkGamePlayer.singleton.GetDisplayName();
        }
    }
}
