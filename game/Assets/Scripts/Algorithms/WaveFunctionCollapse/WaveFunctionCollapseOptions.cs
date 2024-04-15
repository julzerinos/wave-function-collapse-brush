using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Algorithms.WaveFunctionCollapse
{
    [Serializable]
    public class WaveFunctionCollapseOptions
    {
        public int gridSize = 25;
        public float tileOffset = 1;
        [SerializeField] private int seed;
        [SerializeField] private bool useRandomSeed = true;

        public int Seed => useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : seed;
    }
}