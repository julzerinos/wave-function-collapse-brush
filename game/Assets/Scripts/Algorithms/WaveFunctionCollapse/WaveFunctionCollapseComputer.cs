using System.Collections.Generic;
using Algorithms.WaveFunctionCollapse.Input;
using Algorithms.WaveFunctionCollapse.WaveGraph;
using Utility.Graph;
using Random = System.Random;

namespace Algorithms.WaveFunctionCollapse
{
    public class WaveFunctionCollapseComputer
    {
        private readonly IWaveFunctionInput _input;
        private readonly Random _random;
        private readonly Graph<Cell> _waveGraph;

        public WaveFunctionCollapseComputer(IWaveFunctionInput input, WaveFunctionCollapseOptions options)
        {
            _input = input;
            _random = new Random(options.Seed);
            _waveGraph = WaveFunctionCollapse.InitializeWaveGraph(input);
        }

        public IEnumerable<Cell> CompleteGrid()
        {
            return WaveFunctionCollapse.Execute(_waveGraph, _random, _input);
        }

        public IEnumerable<Cell> Expand(Cell patchStartCell, int cellCount)
        {
            foreach (
                var cell
                in
                WaveFunctionCollapse.AddCells(
                    _waveGraph,
                    patchStartCell,
                    cellCount,
                    _input.TileData,
                    _input.NeighborOffsets
                )
            ) yield return cell;

            foreach (var cell in CompleteGrid())
                yield return cell;
        }

        public IEnumerable<Cell> ResetCells(Cell patchStartCell, int cellCount)
        {
            return WaveFunctionCollapse.ResetCells(_waveGraph, patchStartCell, cellCount, _input.TileData);
        }

        public IEnumerable<Cell> ParseResult()
        {
            return WaveFunctionCollapse.ParseAll(_waveGraph);
        }
    }
}