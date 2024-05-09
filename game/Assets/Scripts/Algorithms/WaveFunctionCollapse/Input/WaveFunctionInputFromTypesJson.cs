using System;
using System.Collections.Generic;
using Algorithms.Tilesets;
using UnityEngine;
using Utility.Serialize;

namespace Algorithms.WaveFunctionCollapse.Input
{
    [Serializable]
    public class TileSetJson
    {
        public string[] tiles;
        public float[] probabilities;
        public Vector2[] offsets;
        public NestedArray<NestedArray<int>> types;
    }

    /// <summary>
    /// A configuration file has to have
    /// 1. tiles            string of alphabetically ordered tile names
    /// 2. types            array of array (per tile per direction) of type in each direction
    ///                     types must be in the same order as tiles
    /// 3. probabilities    array of probability for each tile
    /// 4. offsets          physical offset to neighbor center
    /// </summary>
    public class WaveFunctionInputFromTypesJson : IWaveFunctionInput
    {
        public string[] Tiles { get; }
        public TileData[] TileData { get; }
        public int Cardinality { get; }
        public Vector2[] NeighborOffsets { get; }

        public int GetOppositeDirectionIndex(int direction) => (direction + Cardinality / 2) % Cardinality;
        public Dictionary<int, float> ProbabilityLookup { get; }
        public float[] Rotations { get; }

        public WaveFunctionInputFromTypesJson(string configurationJsonPath)
        {
            var textAsset = Resources.Load<TextAsset>(configurationJsonPath) ??
                            throw new Exception(
                                $"[WaveFunctionInputFromJson] Could not find configuration file under path '{configurationJsonPath}'.");

            var tileSetJson = JsonUtility.FromJson<TileSetJson>(textAsset.text);

            Tiles = tileSetJson.tiles;

            var tilesWithTypedDirections = new int[Tiles.Length][];
            for (var tileIndex = 0; tileIndex < Tiles.Length; tileIndex++)
            {
                var directionArrays = tileSetJson.types.array[tileIndex].array;
                tilesWithTypedDirections[tileIndex] = directionArrays;
            }

            NeighborOffsets = tileSetJson.offsets;
            Cardinality = NeighborOffsets.Length;

            var transformations = new TileTransformation[Cardinality];
            Rotations = new float[Cardinality];
            for (var i = 0; i < Cardinality; i++)
            {
                var rotation = i * 360 / (float)Cardinality;

                transformations[i] = new TileTransformation
                {
                    DegreesRotation = rotation,
                    IndexOffset = i
                };
                Rotations[i] = rotation;
            }

            TileData = ConnectionsFromTypesParser.Parse(tilesWithTypedDirections, transformations);

            ProbabilityLookup = new Dictionary<int, float>();
            for (var tileIndex = 0; tileIndex < TileData.Length; tileIndex++)
            {
                var probability = tileSetJson.probabilities[TileData[tileIndex].OriginalIndex];
                if (probability != 0)
                    ProbabilityLookup.Add(tileIndex, tileSetJson.probabilities[TileData[tileIndex].OriginalIndex]);
            }
        }
    }
}