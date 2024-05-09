using System.Collections.Generic;
using Algorithms.Tilesets;
using UnityEngine;

namespace Algorithms.WaveFunctionCollapse.Input
{
    public interface IWaveFunctionInput
    {
        public string[] Tiles { get; }
        public TileData[] TileData { get; }

        public int Cardinality { get; }

        public Vector2[] NeighborOffsets { get; }

        public int GetOppositeDirectionIndex(int direction);

        public Dictionary<int, float> ProbabilityLookup { get; }

        public float[] Rotations { get; }
    }
}