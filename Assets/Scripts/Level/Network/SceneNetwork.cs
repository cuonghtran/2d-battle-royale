using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

namespace MainGame
{
    public class SceneNetwork : NetworkBehaviour
    {
        public TMP_Text canvasStatusText;
        public PlayerNetwork playerNetwork;

        [SyncVar(hook = nameof(OnStatusTextChanged))]
        public string statusText;

        public void ButtonSendMessage()
        {
            if (playerNetwork != null)
                playerNetwork.CmdSendPlayerMessage();
        }

        void OnStatusTextChanged(string _Old, string _New)
        {
            //called from sync var hook, to update info on screen for all players
            canvasStatusText.text = statusText;
        }
    }
}