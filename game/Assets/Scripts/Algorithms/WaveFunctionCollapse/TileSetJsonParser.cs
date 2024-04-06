using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace Algorithms.WaveFunctionCollapse
{
    [Serializable]
    public class TileSetJson
    {
        public int[][][] connections;
        public string[] tiles;
    }

    public class WaveFunctionInputFromJson : IWaveFunctionInput
    {
        public int TileCount { get; }
        public string[] Tiles { get; }
        public Dictionary<int, Dictionary<Direction, int>> ConnectionLookup { get; }

        public WaveFunctionInputFromJson()
        {
            JsonUtility.FromJson<TileSetJson>(Resources.Load());
        }
    }
}