using System.Collections.Generic;
using Algorithms.Tilesets;
using Algorithms.WaveFunctionCollapse.WaveGraph;

namespace Algorithms.WaveFunctionCollapse.Input
{
    public interface IWaveFunctionInput
    {
        public int TileCount { get; }
        public string[] Tiles { get; }
        public TileData[] TileData { get; }

        public int Cardinality { get; }

        public CellCoordinates[] NeighborOffsets { get; }

        public int GetOppositeDirectionIndex(int direction);

        public Dictionary<int, float> ProbabilityLookup { get; }
    }
}