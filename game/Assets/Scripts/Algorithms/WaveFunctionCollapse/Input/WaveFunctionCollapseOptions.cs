using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Algorithms.WaveFunctionCollapse.Input
{
    [Serializable]
    public class WaveFunctionCollapseOptions
    {
        public int initialPatchCount = 50;
        public Vector2Int initialPatchLocation;
        public float tileOffset = 1;
        [SerializeField] private int seed;
        [SerializeField] private bool useRandomSeed = true;
        public int patchCellCount = 50;
        public bool overwritePatch;

        public int Seed => useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : seed;
    }
}