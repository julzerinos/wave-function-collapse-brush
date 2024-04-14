using System;
using System.Collections.Generic;
using UnityEngine;

namespace Algorithms.WaveFunctionCollapse
{
    [Serializable]
    public class NestedArray<T>
    {
        public T[] array;
    }

    [Serializable]
    public class TileSetJson
    {
        public string[] tiles;
        public NestedArray<NestedArray<NestedArray<int>>> connections;
    }

    public class WaveFunctionInputFromJson : IWaveFunctionInput
    {
        public int TileCount { get; }
        public string[] Tiles { get; }
        public Dictionary<int, Dictionary<int, HashSet<int>>> ConnectionLookup { get; }

        public WaveFunctionInputFromJson(string configurationJsonPath)
        {
            var textAsset = Resources.Load<TextAsset>(configurationJsonPath) ??
                            throw new Exception(
                                $"[WaveFunctionInputFromJson] Could not find configuration file under path '{configurationJsonPath}'.");

            var tileSetJson = JsonUtility.FromJson<TileSetJson>(textAsset.text);

            TileCount = tileSetJson.tiles.Length;
            Tiles = tileSetJson.tiles;

            var lookup = new Dictionary<int, Dictionary<int, HashSet<int>>>();
            for (var tileIndex = 0; tileIndex < tileSetJson.connections.array.Length; tileIndex++)
            {
                lookup.Add(tileIndex, new Dictionary<int, HashSet<int>>());

                var directionLookup = lookup[tileIndex];
                var directionArrays = tileSetJson.connections.array[tileIndex].array;
                for (int direction = 0; direction < directionArrays.Length; direction++)
                    directionLookup.Add(direction, new HashSet<int>(directionArrays[direction].array));
            }

            ConnectionLookup = lookup;
        }
    }
}