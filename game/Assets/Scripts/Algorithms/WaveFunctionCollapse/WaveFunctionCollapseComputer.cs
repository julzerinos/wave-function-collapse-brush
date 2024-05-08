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
        }

        public IEnumerable<Cell> CompleteGrid()
        {
            return WaveFunctionCollapse.Execute(_waveGraph, _random, _input);
        }

        public IEnumerable<Cell> Expand(Cell patchStartCell, int cellCount, bool overwrite = false)
        {
            foreach (var cell in WaveFunctionCollapse.AddCells(_waveGraph, patchStartCell, cellCount, _input.TileData,
                         _input.NeighborOffsets, overwrite)) yield return cell;

            foreach (var cell in CompleteGrid())
                yield return cell;
        }

        public IEnumerable<Cell> ResetCells(Cell patchStartCell, int cellCount)
        {
            return WaveFunctionCollapse.ResetCells(_waveGraph, patchStartCell, cellCount, _input.TileData);
        }

        // public void Clear()
        // {
        //     _waveGraph = WaveFunctionCollapse.InitializeWaveGraph(_input, _options);
        //     WaveFunctionCollapse.AddCells(_waveGraph, new CellCoordinates(0, 0), _options.initialPatchCount, _input.TileData);
        //     _random = new Random(_options.Seed);
        // }

        public IEnumerable<Cell> ParseResult()
        {
            return WaveFunctionCollapse.ParseAll(_waveGraph, _input);
        }
    }
}