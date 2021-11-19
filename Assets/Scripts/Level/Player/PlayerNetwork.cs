using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

namespace MainGame
{
    public class PlayerNetwork : NetworkBehaviour
    {
        [HideInInspector] public string PlayerName;
    }
}
