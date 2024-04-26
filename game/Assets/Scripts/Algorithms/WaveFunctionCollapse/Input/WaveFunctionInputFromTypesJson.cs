using System;
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
    }

    public class WaveFunctionInputFromTypesJson : IWaveFunctionInput
    {
        public int TileCount { get; }
        public string[] Tiles { get; }

        public TileData[] TileData { get; }

        // TODO never read from config json
        public int Cardinality { get; } = 4;

        // TODO always assumes square tilesets
        public CellCoordinates[] NeighborOffsets { get; } = new[]
        {
            new CellCoordinates(0f, 1f),
            new CellCoordinates(1f, 0f),
            new CellCoordinates(0f, -1f),
            new CellCoordinates(-1f, 0f)
        };

        public int GetOppositeDirectionIndex(int direction) => (direction + Cardinality / 2) % Cardinality;

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
                var directionLookup = tilesWithTypedDirections[tileIndex] = new int[tileSetJson.types.array[tileIndex].array.Length];
                var directionArrays = tileSetJson.types.array[tileIndex].array;

                tilesWithTypedDirections[tileIndex] = directionArrays;
            }

            TileData = ConnectionsFromTypesParser.Parse(tilesWithTypedDirections);
            TileCount = TileData.Length;
        }
    }
}