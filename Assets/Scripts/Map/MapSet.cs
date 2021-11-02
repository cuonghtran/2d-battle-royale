using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    [CreateAssetMenu(fileName = "New Map Set", menuName = "Map Set")]
    public class MapSet : ScriptableObject
    {
        [Scene] [SerializeField] private List<string> maps = new List<string>();

        public IReadOnlyCollection<string> Maps => maps.AsReadOnly();
    }
}
