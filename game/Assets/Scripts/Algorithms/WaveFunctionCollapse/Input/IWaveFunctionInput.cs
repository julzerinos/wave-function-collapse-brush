using System.Collections.Generic;
using Algorithms.Tilesets;
using Algorithms.WaveFunctionCollapse.WaveGraph;
using Utility.Graph;

namespace Algorithms.WaveFunctionCollapse.Input
{
    public enum TileType
    {
        square,
        hex
    }

    public interface IWaveFunctionInput
    {
        public int TileCount { get; }
        public string[] Tiles { get; }
        public TileData[] TileData { get; }

        public int Cardinality { get; }

        public INodeCoordinates[] NeighborOffsets { get; }

        public TileType TileType { get; }
        public int GetOppositeDirectionIndex(int direction);

        public Dictionary<int, float> ProbabilityLookup { get; }
    }
}