using System;
using Algorithms.WaveFunctionCollapse;
using UnityEngine;

namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        private void Awake()
        {
            var waveFunctionInputFromJson = new WaveFunctionInputFromJson("Models/Tiles/KenneyTileSet1/lookup");
            var waveFunctionResult = WaveFunctionCollapse.Execute(waveFunctionInputFromJson,
                new WaveFunctionCollapseOptions { Seed = 0, GridSize = 25 });
            
            
        }
    }
}