using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithms.WaveFunctionCollapse
{
    public class WaveFunctionCollapseOptions
    {
        // TODO [#1] Implement optional options for the algorithm

        public readonly int GridSize = 25;
        public readonly int Seed = 25;
    }

    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    public static class WaveFunctionCollapse
    {
        private static Random _random;

        public static IWaveFunctionResult Execute(IWaveFunctionInput input, WaveFunctionCollapseOptions options)
        {
            _random = new Random(options.Seed);

            var waveGrid = InitializeWaveGrid(options.GridSize, input.TileCount);

            var startCollapseRow = _random.Next(0, options.GridSize);
            var startCollapseCol = _random.Next(0, options.GridSize);
            var collapsePosition = (startCollapseCol, startCollapseRow);

            while (true)
            {
                Collapse(waveGrid[startCollapseCol, startCollapseRow]);
                Propagate(waveGrid, connectionLookup, collapsePosition);
                if (!Observe(waveGrid, out collapsePosition))
                    break;
            }
            
            
        }

        private static bool Observe(HashSet<int>[,] waveGrid, out (int, int) position)
        {
            return FindLowestEntropyCellPosition(waveGrid, out position);
        }

        private static void Collapse(HashSet<int> cell)
        {
            var tile = PickTile(cell);
            cell.Clear();
            cell.Add(tile);
        }

        private static void Propagate(HashSet<int>[,] waveGrid,
            Dictionary<int, Dictionary<Direction, HashSet<int>>> connectionLookup, (int, int) seedPosition)
        {
            var neighboringCells = new Stack<(int, int)>();
            neighboringCells.Push(seedPosition);

            while (neighboringCells.Count > 0)
            {
                var (col, row) = neighboringCells.Pop();
                var cell = waveGrid[col, row];

                if (cell.Count == 0)
                    throw new Exception(
                        "[WaveFunctionCollapse > Propagate] Wave collapse encountered failed superposition");

                // TODO [#3] Replace with graph structure
                var neighbors = new[]
                {
                    ((1, 0), Direction.North),
                    ((-1, 0), Direction.East),
                    ((0, 1), Direction.South),
                    ((0, -1), Direction.West),
                };

                foreach (var (neighborDirection, directionName) in neighbors)
                {
                    var neighborPosition = (col + neighborDirection.Item1, row + neighborDirection.Item2);
                    if (neighborPosition.Item1 is > 25 or < 0 || neighborPosition.Item2 is > 25 or < 0)
                        continue;

                    var neighborCell = waveGrid[neighborPosition.Item1, neighborPosition.Item2];
                    var neighborTileOptions = new HashSet<int>();

                    foreach (var tile in neighborCell)
                        neighborTileOptions.UnionWith(connectionLookup[tile][directionName]);

                    var newSuperposition = new HashSet<int>(cell);
                    newSuperposition.IntersectWith(neighborTileOptions);

                    if (cell.SetEquals(newSuperposition))
                        continue;

                    cell = newSuperposition;
                    neighboringCells.Push(neighborPosition);
                }
            }
        }

        private static bool FindLowestEntropyCellPosition(HashSet<int>[,] waveGrid,
            out (int, int) positionForMinEntropy)
        {
            var minimumEntropy = int.MaxValue;
            positionForMinEntropy = (-1, -1);

            for (var col = 0; col < waveGrid.GetLength(0); col++)
            for (var row = 0; row < waveGrid.GetLength(1); row++)
            {
                var cell = waveGrid[col, row];

                if (cell.Count >= minimumEntropy)
                    continue;

                positionForMinEntropy = (col, row);

                if (cell.Count == 2)
                    return true;

                minimumEntropy = cell.Count;
                positionForMinEntropy = (col, row);
            }

            return minimumEntropy > 1;
        }

        private static HashSet<int>[,] InitializeWaveGrid(int gridSize, int tileCount)
        {
            var waveGrid = new HashSet<int>[gridSize, gridSize];
            for (var i = 0; i < gridSize; i++)
            for (var j = 0; j < gridSize; j++)
                waveGrid[i, j] = new HashSet<int>(Enumerable.Range(0, tileCount));
            return waveGrid;
        }

        // TODO [#2] Add tile probability distribution
        private static int PickTile(ICollection<int> cell)
        {
            return cell.ElementAt(_random.Next(cell.Count));
        }
    }
}