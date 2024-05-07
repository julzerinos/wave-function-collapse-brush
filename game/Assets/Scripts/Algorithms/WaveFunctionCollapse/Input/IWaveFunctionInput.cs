using System.Collections.Generic;
using Algorithms.Tilesets;
using Algorithms.WaveFunctionCollapse.WaveGraph;
using UnityEngine;
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

        public Vector2[] NeighborOffsets { get; }

        public int GetOppositeDirectionIndex(int direction);

        public Dictionary<int, float> ProbabilityLookup { get; }
    }
}