using System.Collections.Generic;
using System.Linq;
using Algorithms.Tilesets;
using Algorithms.WaveFunctionCollapse.Input;
using Algorithms.WaveFunctionCollapse.WaveGraph;
using UnityEngine;
using Utility.Graph;
using Random = System.Random;

namespace Algorithms.WaveFunctionCollapse
{
    public class WaveFunctionCollapseComputer
    {
        private readonly IWaveFunctionInput _input;
        private readonly WaveFunctionCollapseOptions _options;
        private Random _random;
        private Graph<Cell> _waveGraph;

        public WaveFunctionCollapseComputer(IWaveFunctionInput input, WaveFunctionCollapseOptions options)
        {
            _input = input;
            _options = options;
            _random = new Random(_options.Seed);
            _waveGraph = WaveFunctionCollapse.InitializeWaveGraph(input, options);
            // WaveFunctionCollapse.AddCells(_waveGraph, new CellCoordinates(0, 0), options.defaultPatchCellCount, _input.TileData);
        }

        public IEnumerable<(TileData, INodeCoordinates)> CompleteGrid()
        {
            return WaveFunctionCollapse.Execute(_waveGraph, _random, _input);
        }

        public IEnumerable<(TileData, INodeCoordinates)> Expand(Vector2Int patchCenter, int cellCount, bool overwrite = false)
        {
            var seedPosition = _input.TileType == TileType.hex
                ? (INodeCoordinates)new HexagonCellCoordinates(patchCenter)
                : new SquareCellCoordinates(patchCenter);

            return WaveFunctionCollapse.AddCells(_waveGraph, seedPosition, cellCount, _input.TileData, overwrite);
        }

        // public void Clear()
        // {
        //     _waveGraph = WaveFunctionCollapse.InitializeWaveGraph(_input, _options);
        //     WaveFunctionCollapse.AddCells(_waveGraph, new CellCoordinates(0, 0), _options.initialPatchCount, _input.TileData);
        //     _random = new Random(_options.Seed);
        // }

        public IEnumerable<(TileData, INodeCoordinates)> ParseResult()
        {
            return WaveFunctionCollapse.ParseAll(_waveGraph, _input);
        }
    }
}