using System.Collections.Generic;

namespace Algorithms.WaveFunctionCollapse
{
    public interface IWaveFunctionInput
    {
        public int TileCount { get; }
        public string[] Tiles { get; }
        public Dictionary<int, Dictionary<Direction, int>> ConnectionLookup { get; }
        
        
    }
}