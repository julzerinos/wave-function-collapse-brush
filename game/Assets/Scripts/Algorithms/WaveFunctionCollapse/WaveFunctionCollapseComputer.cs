using System.Collections.Generic;
using System.Linq;
using Algorithms.Tilesets;
using Algorithms.WaveFunctionCollapse.Input;
using UnityEngine;
using Random = System.Random;

namespace Algorithms.WaveFunctionCollapse
{
    public class WaveFunctionCollapseComputer
    {
        private readonly IWaveFunctionInput _input;
        private readonly WaveFunctionCollapseOptions _options;
        private Random _random;
        private HashSet<int>[,] _waveGrid;


        public WaveFunctionCollapseComputer(IWaveFunctionInput input, WaveFunctionCollapseOptions options)
        {
            _input = input;
            _options = options;
            _random = new Random(_options.Seed);
            _waveGrid = WaveFunctionCollapse.InitializeWaveGrid(options.gridSize, input.TileCount);
        }

        public void CompleteGrid()
        {
            WaveFunctionCollapse.Execute(ref _waveGrid, _random, _input, _options);
        }

        public void UnCollapseCells((int col, int row) colRowPosition, int maxCells)
        {
            var neighboringCells = new Queue<(int col, int row)>();
            neighboringCells.Enqueue(colRowPosition);

            var positionsToFix = new Stack<(int col, int row)>();
            var explored = new HashSet<(int col, int row)>();

            var cellsReCollapsed = 0;

            while (neighboringCells.Count > 0 && cellsReCollapsed < maxCells)
            {
                var (col, row) = neighboringCells.Dequeue();

                positionsToFix.Push((col, row));
                explored.Add((col, row));
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
                    if (explored.Contains(neighborPosition) || neighborPosition.col is >= 25 or < 0 || neighborPosition.row is >= 25 or < 0)
                        continue;

                    neighboringCells.Enqueue(neighborPosition);
                }
            }

            while (positionsToFix.Count > 0)
            {
                var (col, row) = positionsToFix.Pop();
                var cell = _waveGrid[col, row];

                if (cell.Count == 0)
                {
                    Debug.LogWarning(
                        "[WaveFunctionCollapse > Propagate] Wave collapse encountered failed superposition (skipping)."
                    );
                    continue;
                }

                // TODO [#3] Replace with graph structure
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

                    var neighborCell = _waveGrid[neighborPosition.col, neighborPosition.row];

                    var constrainedCell = new HashSet<int>();
                    var oppositeDirection = (directionName + neighbors.Length / 2) % neighbors.Length;

                    if (!explored.Contains(neighborDirection))
                        foreach (var neighborTile in neighborCell)
                            constrainedCell.UnionWith(_input.TileData[neighborTile].ConnectionsPerDirection[oppositeDirection]);

                    cell.IntersectWith(constrainedCell);
                }


                // foreach (var (neighborDirection, directionName) in neighbors)
                // {
                //     var neighborPosition = (col: col + neighborDirection.col, row: row + neighborDirection.row);
                //     if (neighborPosition.col is >= 25 or < 0 || neighborPosition.row is >= 25 or < 0)
                //         continue;
                //
                //     if (!explored.Contains(neighborDirection))
                //         continue;
                //
                //     var neighborCell = _waveGrid[neighborPosition.col, neighborPosition.row];
                //
                //     if (!WaveFunctionCollapse.MatchNeighbor(
                //             neighborCell,
                //             cell.Select(t => _input.ConnectionLookup[t][directionName]),
                //             out var superposition)
                //        )
                //         continue;
                //
                //     _waveGrid[neighborPosition.col, neighborPosition.row] = superposition;
                // }
            }
        }

        public void Clear()
        {
            _waveGrid = WaveFunctionCollapse.InitializeWaveGrid(_options.gridSize, _input.TileCount);
            _random = new Random(_options.Seed);
        }

        public IEnumerable<(TileData, (int col, int row))> ParseResult()
        {
            return WaveFunctionCollapse.Parse(_waveGrid, _input, _options);
        }
    }
}