using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Algorithms.WaveFunctionCollapse
{
    public static class WaveFunctionCollapse
    {
        public static HashSet<int>[,] InitializeWaveGrid(int gridSize, int tileCount)
        {
            var waveGrid = new HashSet<int>[gridSize, gridSize];
            for (var i = 0; i < gridSize; i++)
            for (var j = 0; j < gridSize; j++)
                waveGrid[i, j] = new HashSet<int>(Enumerable.Range(0, tileCount));
            return waveGrid;
        }

        private static bool Observe(HashSet<int>[,] waveGrid, out (int, int) position)
        {
            return FindLowestEntropyCellPosition(waveGrid, out position);
        }

        private static void Collapse(HashSet<int> cell, Random random)
        {
            var tile = PickTile(cell, random);
            cell.Clear();
            cell.Add(tile);
        }

        private static void Propagate(
            HashSet<int>[,] waveGrid,
            IReadOnlyDictionary<int, Dictionary<int, HashSet<int>>> connectionLookup,
            (int, int) seedPosition
        )
        {
            var neighboringCells = new Stack<(int, int)>();
            neighboringCells.Push(seedPosition);

            while (neighboringCells.Count > 0)
            {
                var (col, row) = neighboringCells.Pop();
                var cell = waveGrid[col, row];

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

                    var neighborCell = waveGrid[neighborPosition.col, neighborPosition.row];
                    var neighborTileOptions = new HashSet<int>();

                    foreach (var tile in cell)
                        neighborTileOptions.UnionWith(connectionLookup[tile][directionName]);

                    var newSuperposition = new HashSet<int>(neighborCell);
                    newSuperposition.IntersectWith(neighborTileOptions);

                    if (neighborCell.SetEquals(newSuperposition))
                        continue;

                    waveGrid[neighborPosition.col, neighborPosition.row] = newSuperposition;
                    neighboringCells.Push(neighborPosition);
                }
            }
        }

        private static bool FindLowestEntropyCellPosition(
            HashSet<int>[,] waveGrid,
            out (int, int) positionForMinEntropy
        )
        {
            var minimumEntropy = int.MaxValue;
            positionForMinEntropy = (-1, -1);

            for (var col = 0; col < waveGrid.GetLength(0); col++)
            for (var row = 0; row < waveGrid.GetLength(1); row++)
            {
                var cell = waveGrid[col, row];

                if (cell.Count >= minimumEntropy || cell.Count <= 1)
                    continue;

                positionForMinEntropy = (col, row);

                if (cell.Count == 2)
                    return true;

                minimumEntropy = cell.Count;
                positionForMinEntropy = (col, row);
            }

            return minimumEntropy < int.MaxValue;
        }

        // TODO [#2] Add tile probability distribution
        private static int PickTile(ICollection<int> cell, Random random)
        {
            if (cell.Count == 1) return cell.ElementAt(0);

            var rand = random.NextDouble();
            var prob = new[] { .7, .75, .8, .85, .9, .95, 1.0 };

            for (var i = 0; i < 7; i++)
                if (cell.Contains(i) && rand <= prob[i])
                    return i;

            return cell.ElementAt(0);
        }

        public static WaveFunctionResult Parse(HashSet<int>[,] waveGrid, IWaveFunctionInput input)
        {
            var tileGrid = new string[25, 25];

            for (var col = 0; col < tileGrid.GetLength(0); col++)
            for (var row = 0; row < tileGrid.GetLength(1); row++)
            {
                var cell = waveGrid[col, row];
                tileGrid[col, row] = cell.Count == 1 ? input.Tiles[cell.ElementAt(0)] : "";
            }

            return new WaveFunctionResult { TileGrid = tileGrid };
        }

        public static void Execute(
            ref HashSet<int>[,] waveGrid, Random random, IWaveFunctionInput input, WaveFunctionCollapseOptions options
        )
        {
            var startCollapseRow = random.Next(0, options.gridSize);
            var startCollapseCol = random.Next(0, options.gridSize);
            var collapsePosition = (collapseCol: startCollapseCol, collapseRow: startCollapseRow);

            var iterations = 0;

            do
            {
                Collapse(waveGrid[collapsePosition.collapseCol, collapsePosition.collapseRow], random);
                Propagate(waveGrid, input.ConnectionLookup, collapsePosition);

                if (iterations++ <= 1000) continue;

                Debug.LogWarning($"[WaveFunctionCollapse] WFC loop iterated the maximum number of iterations.");
                break;
            } while (Observe(waveGrid, out collapsePosition));
        }

        public static WaveFunctionResult Generate(IWaveFunctionInput input, WaveFunctionCollapseOptions options)
        {
            var random = new Random(options.seed);

            var waveGrid = InitializeWaveGrid(options.gridSize, input.TileCount);

            Execute(ref waveGrid, random, input, options);

            return Parse(waveGrid, input);
        }
    }
}