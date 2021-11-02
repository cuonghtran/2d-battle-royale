using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MainGame
{
    public class MapHandler
    {
        private readonly IReadOnlyCollection<string> maps;
        private readonly int _numberOfRounds;

        private int _currentRound;
        private List<string> RemainingMaps;

        public MapHandler(MapSet mapSet, int numberOfRounds)
        {
            maps = mapSet.Maps;
            _numberOfRounds = numberOfRounds;

            ResetMaps();
        }

        private void ResetMaps()
        {
            RemainingMaps = maps.ToList();
        }

        public bool IsComplete => _currentRound == _numberOfRounds;

        public string NextMap()
        {
            if (IsComplete) return null;
            _currentRound++;

            if (RemainingMaps.Count == 0)
                ResetMaps();

            string map = RemainingMaps[Random.Range(0, RemainingMaps.Count)];
            RemainingMaps.Remove(map);
            return map;
        }
    }
}
