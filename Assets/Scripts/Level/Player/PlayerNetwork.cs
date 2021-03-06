using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;
using TMPro;

namespace MainGame
{
    public class PlayerNetwork : NetworkBehaviour
    {
        public TMP_Text playerNameText;

        private Material _playerMaterialClone;

        [SyncVar(hook = nameof(OnNameChanged))]
        public string playerName;
        [SyncVar(hook = nameof(OnColorChanged))]
        public Color playerColor = Color.white;

        public override void OnStartLocalPlayer()
        {
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 0, -10);

            string name = "Player" + Random.Range(100, 999);
            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            CmdSetupPlayer(name, color);
        }

        [Command]
        public void CmdSetupPlayer(string _name, Color _col)
        {
            // player info sent to server, then server updates sync vars which handles it on all clients
            playerName = _name;
            playerColor = _col;
        }

        void OnNameChanged(string _Old, string _New)
        {
            playerNameText.text = playerName;
        }

        void OnColorChanged(Color _Old, Color _New)
        {
            playerNameText.color = _New;
            _playerMaterialClone = new Material(GetComponent<Renderer>().material);
            _playerMaterialClone.color = _New; GetComponent<Renderer>().material = _playerMaterialClone;
        }
    }
}
