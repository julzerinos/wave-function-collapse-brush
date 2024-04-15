using System;
using System.Collections.Generic;
using Algorithms.Tilesets;
using UnityEngine;
using Utility.Serialize;

namespace Algorithms.WaveFunctionCollapse.Input
{
    public class WaveFunctionInputFromConnectionsJson : IWaveFunctionInput
    {
        [Serializable]
        private class TileSetJson
        {
            public string[] tiles;
            public NestedArray<NestedArray<NestedArray<int>>> connections;
        }

        public int TileCount { get; }
        public string[] Tiles { get; }
        public TileData[] TileData { get; }
        // public Dictionary<int, Dictionary<int, HashSet<int>>> ConnectionLookup { get; }

        public WaveFunctionInputFromConnectionsJson(string configurationJsonPath)
        {
            var textAsset = Resources.Load<TextAsset>(configurationJsonPath) ??
                            throw new Exception(
                                $"[WaveFunctionInputFromJson] Could not find configuration file under path '{configurationJsonPath}'.");

            var tileSetJson = JsonUtility.FromJson<TileSetJson>(textAsset.text);

            TileCount = tileSetJson.tiles.Length;
            Tiles = tileSetJson.tiles;

            var lookup = new List<TileData>();
            for (var tileIndex = 0; tileIndex < tileSetJson.connections.array.Length; tileIndex++)
            {
                var directionArrays = tileSetJson.connections.array[tileIndex].array;
                var directionLookup = new HashSet<int>[directionArrays.Length];
                for (var direction = 0; direction < directionArrays.Length; direction++)
                    directionLookup[direction] = new HashSet<int>(directionArrays[direction].array);

                lookup.Add(new TileData
                {
                    Transformation = TileTransformation.Original,
                    OriginalIndex = tileIndex,
                    ConnectionsPerDirection = directionLookup
                });
            }

            TileData = lookup.ToArray();
        }
    }
}