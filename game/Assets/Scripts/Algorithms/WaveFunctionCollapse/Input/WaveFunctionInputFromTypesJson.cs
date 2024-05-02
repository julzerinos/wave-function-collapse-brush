using System;
using System.Collections.Generic;
using Algorithms.Tilesets;
using Algorithms.WaveFunctionCollapse.WaveGraph;
using UnityEngine;
using Utility.Serialize;

namespace Algorithms.WaveFunctionCollapse.Input
{
    [Serializable]
    public class TileSetJson
    {
        public string[] tiles;
        public NestedArray<NestedArray<int>> types;
        public float[] probabilities;
        public CellCoordinates[] offsets;
    }

    /// <summary>
    /// Configuration files has to have
    /// 1. tiles    string of alphabetically ordered tile names
    /// 2. types    array of array (per tile per direction) of type in each direction
    ///             types must be in the same order as tiles
    /// 3. probab   array of probability for each tile
    /// 4. offsets  offset cell coordinate to neighbor
    /// </summary>
    public class WaveFunctionInputFromTypesJson : IWaveFunctionInput
    {
        public int TileCount { get; }
        public string[] Tiles { get; }

        public TileData[] TileData { get; }

        // TODO never read from config json
        public int Cardinality { get; }

        // TODO always assumes square tilesets
        public CellCoordinates[] NeighborOffsets { get; }
        // new[]
        // {
        //     new CellCoordinates(0f, 1f),
        //     new CellCoordinates(1f, 0f),
        //     new CellCoordinates(0f, -1f),
        //     new CellCoordinates(-1f, 0f)
        // };

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

            var tilesWithTypedDirections = new int[tileSetJson.types.array.Length][];
            for (var tileIndex = 0; tileIndex < tileSetJson.types.array.Length; tileIndex++)
            {
                tilesWithTypedDirections[tileIndex] = new int[tileSetJson.types.array[tileIndex].array.Length];
                var directionArrays = tileSetJson.types.array[tileIndex].array;

                tilesWithTypedDirections[tileIndex] = directionArrays;
            }

            NeighborOffsets = tileSetJson.offsets;
            Cardinality = NeighborOffsets.Length;

            var transformations = new TileTransformation[Cardinality];
            for (var i = 0; i < Cardinality; i++)
            {
                transformations[i] = new TileTransformation
                {
                    DegreesRotation = i * 360 / (float)Cardinality,
                    IndexOffset = i
                };
            }

            TileData = ConnectionsFromTypesParser.Parse(tilesWithTypedDirections, transformations);
            TileCount = TileData.Length;


            ProbabilityLookup = new Dictionary<int, float>();

            for (var tileIndex = 0; tileIndex < TileData.Length; tileIndex++)
                ProbabilityLookup.Add(tileIndex, tileSetJson.probabilities[TileData[tileIndex].OriginalIndex]);
        }
    }
}