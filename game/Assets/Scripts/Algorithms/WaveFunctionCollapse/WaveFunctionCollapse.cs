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
    public static class WaveFunctionCollapse
    {
        public static Graph<Cell, CellCoordinates> InitializeWaveGraph(IWaveFunctionInput input, WaveFunctionCollapseOptions options)
        {
            var waveGraph = new Graph<Cell, CellCoordinates>(GetOppositeDirectionIndex);

            var startPosition = new CellCoordinates { X = 0, Y = 0 };
            var startCellNode = new Node<Cell, CellCoordinates>(input.Cardinality, Cell.Factory(input.TileCount), startPosition);

            var positionFrontier = new HashSet<CellCoordinates> { startPosition };
            var nodesQueue = new Queue<Node<Cell, CellCoordinates>>();
            nodesQueue.Enqueue(startCellNode);

            while (nodesQueue.Count > 0)
            {
                var node = nodesQueue.Dequeue();
                waveGraph.AddNode(node);

                for (var directionIndex = 0; directionIndex < input.Cardinality; directionIndex++)
                {
                    if (!GetNeighborPosition(node.Coordinates, directionIndex, out var neighborPosition))
                        continue;

                    var isNodeInGraph = waveGraph.GetNode(neighborPosition, out var neighborNode);
                    if (!isNodeInGraph)
                        neighborNode = new Node<Cell, CellCoordinates>(4, Cell.Factory(input.TileCount), neighborPosition);

                    node.RegisterNeighbor(neighborNode, directionIndex);
                    neighborNode.RegisterNeighbor(node, GetOppositeDirectionIndex(directionIndex));

                    if (isNodeInGraph || !positionFrontier.Add(neighborPosition))
                        continue;

                    nodesQueue.Enqueue(neighborNode);
                }
            }

            return waveGraph;

            // TODO from input
            int GetOppositeDirectionIndex(int direction) => (direction + input.Cardinality / 2) % input.Cardinality;

            // TODO from input
            bool GetNeighborPosition(
                CellCoordinates centerPosition,
                int directionIndex,
                out CellCoordinates neighborPosition
            )
            {
                // TODO should come from input
                var neighbors = new[]
                {
                    new CellCoordinates(0f, 1f),
                    new CellCoordinates(1f, 0f),
                    new CellCoordinates(0f, -1f),
                    new CellCoordinates(-1f, 0f)
                };

                neighborPosition = new CellCoordinates(float.MaxValue, float.MaxValue);

                neighborPosition = centerPosition + neighbors[directionIndex];
                return neighborPosition.X < options.gridSize &&
                       neighborPosition.Y < options.gridSize &&
                       neighborPosition is { X: >= 0, Y: >= 0 };
            }
        }

        private static bool Observe(Graph<Cell, CellCoordinates> waveGraph, out Node<Cell, CellCoordinates> node)
        {
            return FindLowestEntropyNode(waveGraph, out node);
        }

        private static void Collapse(Cell cell, Random random)
        {
            var tile = PickTile(cell, random);
            cell.Clear();
            cell.Add(tile);
        }

        public static bool MatchNeighbor(
            Cell neighborCell,
            IEnumerable<HashSet<int>> possibleNeighborTiles,
            out HashSet<int> superposition
        )
        {
            superposition = new HashSet<int>(neighborCell);

            if (neighborCell.Count <= 1) return false;

            var neighborTileOptions = new HashSet<int>();
            foreach (var tileOptions in possibleNeighborTiles)
                neighborTileOptions.UnionWith(tileOptions);
            superposition.IntersectWith(neighborTileOptions);

            return !neighborCell.SetEquals(superposition);
        }

        private static void Propagate(
            Graph<Cell, CellCoordinates> waveGraph,
            IReadOnlyList<TileData> tileData,
            Node<Cell, CellCoordinates> seedNode
        )
        {
            var neighboringNodes = new Stack<Node<Cell, CellCoordinates>>();
            neighboringNodes.Push(seedNode);

            while (neighboringNodes.Any())
            {
                var node = neighboringNodes.Pop();
                var cell = node.Content;

                if (!cell.Any())
                {
                    Debug.LogWarning(
                        "[WaveFunctionCollapse > Propagate] Wave collapse encountered failed superposition (skipping)."
                    );
                    continue;
                }

                foreach (var (neighborNode, direction) in node.Neighbors)
                {
                    var neighborCell = neighborNode.Content;

                    var isSuperpositionNew = MatchNeighbor(
                        neighborCell,
                        cell.Select(t => tileData[t].ConnectionsPerDirection[direction]),
                        out var superposition
                    );

                    if (!isSuperpositionNew)
                        continue;

                    neighborNode.Content = new Cell(superposition);
                    neighboringNodes.Push(neighborNode);
                }
            }
        }

        private static bool FindLowestEntropyNode(
            Graph<Cell, CellCoordinates> waveGraph,
            out Node<Cell, CellCoordinates> lowestEntropyNode
        )
        {
            var minimumEntropy = int.MaxValue;
            lowestEntropyNode = null;

            foreach (var node in waveGraph)
            {
                var cell = node.Content;

                if (cell.Count >= minimumEntropy || cell.Count <= 1)
                    continue;

                lowestEntropyNode = node;
                minimumEntropy = cell.Count;

                if (cell.Count == 2)
                    return true;
            }

            return minimumEntropy < int.MaxValue;
        }

        private static int PickTile(Cell cell, Random random)
        {
            if (cell.Count == 1) return cell.ElementAt(0);

            // TODO add tile probability distribution
            var randIndex = random.Next(0, cell.Count);
            return cell.ElementAt(randIndex);
        }

        public static IEnumerable<(TileData, CellCoordinates)> Parse(
            Graph<Cell, CellCoordinates> waveGrid,
            IWaveFunctionInput input
        )
        {
            foreach (var node in waveGrid)
            {
                var cell = node.Content;
                var tile = cell.Count == 1 ? input.TileData[cell.ElementAt(0)] : null;

                yield return (tile, node.Coordinates);
            }
        }

        public static void Execute(
            Graph<Cell, CellCoordinates> waveGraph,
            Random random,
            IWaveFunctionInput input,
            Node<Cell, CellCoordinates> collapseNode
        )
        {
            var iterations = 0;

            do
            {
                Collapse(collapseNode.Content, random);
                Propagate(waveGraph, input.TileData, collapseNode);

                if (iterations++ <= 1000) continue;

                Debug.LogWarning($"[WaveFunctionCollapse] WFC loop iterated the maximum number of iterations.");
                break;
            } while (Observe(waveGraph, out collapseNode));
        }

        public static void Execute(
            Graph<Cell, CellCoordinates> waveGraph,
            Random random,
            IWaveFunctionInput input
        )
        {
            var startCollapseNode = waveGraph.GetRandomNode();
            Execute(waveGraph, random, input, startCollapseNode);
        }

        public static IEnumerable<(TileData, CellCoordinates)> Generate(IWaveFunctionInput input, WaveFunctionCollapseOptions options)
        {
            var random = new Random(options.Seed);

            var waveGraph = InitializeWaveGraph(input, options);

            Execute(waveGraph, random, input);

            return Parse(waveGraph, input);
        }
    }
}