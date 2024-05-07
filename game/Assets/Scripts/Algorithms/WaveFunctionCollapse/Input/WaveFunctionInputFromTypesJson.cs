using System;
using System.Collections.Generic;
using System.Linq;
using Algorithms.Tilesets;
using Algorithms.WaveFunctionCollapse.WaveGraph;
using UnityEngine;
using Utility.Graph;
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
    /// Configuration files has to have
    /// 1. tiles    string of alphabetically ordered tile names
    /// 2. types    array of array (per tile per direction) of type in each direction
    ///             types must be in the same order as tiles
    /// 3. probab   array of probability for each tile
    /// 4. offsets  physical offset to edge connector point
    /// </summary>
    public class WaveFunctionInputFromTypesJson : IWaveFunctionInput
    {
        public int TileCount { get; }
        public string[] Tiles { get; }
        public TileData[] TileData { get; }
        public int Cardinality { get; }
        public Vector2[] NeighborOffsets { get; }

        public int GetOppositeDirectionIndex(int direction) => (direction + Cardinality / 2) % Cardinality;
        public Dictionary<int, float> ProbabilityLookup { get; }

        public WaveFunctionInputFromTypesJson(string configurationJsonPath)
        {
            var textAsset = Resources.Load<TextAsset>(configurationJsonPath) ??
                            throw new Exception(
                                $"[WaveFunctionInputFromJson] Could not find configuration file under path '{configurationJsonPath}'.");

            var tileSetJson = JsonUtility.FromJson<TileSetJson>(textAsset.text);

            TileCount = tileSetJson.tiles.Length;
            Tiles = tileSetJson.tiles;

            var tilesWithTypedDirections = new int[Tiles.Length][];
            for (var tileIndex = 0; tileIndex < Tiles.Length; tileIndex++)
            {
                // tilesWithTypedDirections[tileIndex] = new int[tileSetJson.types.array[tileIndex].array.Length];
                var directionArrays = tileSetJson.types.array[tileIndex].array;
                tilesWithTypedDirections[tileIndex] = directionArrays;
            }

            NeighborOffsets = tileSetJson.offsets;
            Cardinality = NeighborOffsets.Length;

            // var transformations = new TileTransformation[Cardinality];
            // for (var i = 0; i < Cardinality; i++)
            // {
            //     transformations[i] = new TileTransformation
            //     {
            //         DegreesRotation = i * 360 / (float)Cardinality,
            //         IndexOffset = i
            //     };
            // }
            var transformations = new TileTransformation[1];
            transformations[0] = new TileTransformation { DegreesRotation = 0, IndexOffset = 0 };

            TileData = ConnectionsFromTypesParser.Parse(tilesWithTypedDirections, transformations);
            TileCount = TileData.Length;

            ProbabilityLookup = new Dictionary<int, float>();
            for (var tileIndex = 0; tileIndex < TileData.Length; tileIndex++)
                ProbabilityLookup.Add(tileIndex, tileSetJson.probabilities[TileData[tileIndex].OriginalIndex]);
        }
    }
}