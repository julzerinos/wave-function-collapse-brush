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
        private Graph<Cell, CellCoordinates> _waveGraph;

        public WaveFunctionCollapseComputer(IWaveFunctionInput input, WaveFunctionCollapseOptions options)
        {
            _input = input;
            _options = options;
            _random = new Random(_options.Seed);
            _waveGraph = WaveFunctionCollapse.InitializeWaveGraph(input, options);
            // WaveFunctionCollapse.AddCells(_waveGraph, new CellCoordinates(0, 0), options.defaultPatchCellCount, _input.TileData);
        }

        public IEnumerable<(TileData, CellCoordinates)> CompleteGrid()
        {
            return WaveFunctionCollapse.Execute(_waveGraph, _random, _input);
        }

        public IEnumerable<(TileData, CellCoordinates)> Expand(CellCoordinates patchCenter, int cellCount, bool overwrite = false)
        {
            return WaveFunctionCollapse.AddCells(_waveGraph, patchCenter, cellCount, _input.TileData, overwrite);
        }

        // public void Clear()
        // {
        //     _waveGraph = WaveFunctionCollapse.InitializeWaveGraph(_input, _options);
        //     WaveFunctionCollapse.AddCells(_waveGraph, new CellCoordinates(0, 0), _options.initialPatchCount, _input.TileData);
        //     _random = new Random(_options.Seed);
        // }

        public IEnumerable<(TileData, CellCoordinates)> ParseResult()
        {
            return WaveFunctionCollapse.ParseAll(_waveGraph, _input);
        }
    }
}