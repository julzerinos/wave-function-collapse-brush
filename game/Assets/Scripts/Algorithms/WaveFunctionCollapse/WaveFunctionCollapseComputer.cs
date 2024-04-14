using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Algorithms.WaveFunctionCollapse
{
    public class WaveFunctionCollapseComputer
    {
        private readonly IWaveFunctionInput _input;
        private readonly WaveFunctionCollapseOptions _options;
        private readonly Random _random;
        private HashSet<int>[,] _waveGrid;


        public WaveFunctionCollapseComputer(IWaveFunctionInput input, WaveFunctionCollapseOptions options)
        {
            _input = input;
            _options = options;
            _random = new Random(options.seed);
            _waveGrid = WaveFunctionCollapse.InitializeWaveGrid(options.gridSize, input.TileCount);
        }

        public void CompleteGrid()
        {
            WaveFunctionCollapse.Execute(ref _waveGrid, _random, _input, _options);
        }

        private void ReCollapseCells((int col, int row) colRowPosition, int maxCells)
        {
            var neighboringCells = new Stack<(int col, int row)>();
            neighboringCells.Push(colRowPosition);

            var cellsReCollapsed = 0;

            while (neighboringCells.Count > 0 && cellsReCollapsed < maxCells)
            {
                var (col, row) = neighboringCells.Pop();

                _waveGrid[col, row] = new HashSet<int>(Enumerable.Range(0, _input.TileCount));
                cellsReCollapsed++;

                var neighbors = new[]
                {
                    ((col: 1, row: 0), 1),
                    ((col: -1, row: 0), 3),
                    ((col: 0, row: 1), 0),
                    ((col: 0, row: -1), 2),
                };

                foreach (var (neighborDirection, directionName) in neighbors)
                {
                    var neighborPosition = (col: col + neighborDirection.col, row: row + neighborDirection.row);
                    if (neighborPosition.col is >= 25 or < 0 || neighborPosition.row is >= 25 or < 0)
                        continue;

                    neighboringCells.Push(neighborPosition);
                }
            }
        }

        public WaveFunctionResult ParseResult()
        {
            return WaveFunctionCollapse.Parse(_waveGrid, _input);
        }
    }
}