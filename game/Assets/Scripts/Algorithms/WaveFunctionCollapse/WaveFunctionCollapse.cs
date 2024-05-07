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
        public static Graph<Cell> InitializeWaveGraph(
            IWaveFunctionInput input,
            WaveFunctionCollapseOptions options
        )
        {
            var waveGraph = new Graph<Cell>(
                input.GetOppositeDirectionIndex,
                input.Cardinality
            );

            return waveGraph;
        }

        public static IEnumerable<Cell> AddCells(
            Graph<Cell> waveGraph,
            Cell seedCell,
            int cellCount,
            TileData[] tileData,
            Vector2[] offsets,
            bool overwrite = false
        )
        {
            if (!waveGraph.GetNode(seedCell, out var seedNode))
            {
                seedNode = new Node<Cell>(waveGraph.NodeCardinality, seedCell);
                waveGraph.AddNode(seedNode);
            }

            var cellsFrontier = new HashSet<Cell> { seedCell };
            var nodesQueue = new Queue<Node<Cell>>();
            nodesQueue.Enqueue(seedNode);

            while (nodesQueue.Count > 0)
            {
                var node = nodesQueue.Dequeue();

                foreach (var (offset, direction) in offsets.Select((o, i) => (o, i)))
                {
                    var neighborPosition = node.Content.PhysicalPosition + offset;
                    var neighborCell = new Cell(tileData.Length, neighborPosition);

                    var doesNodeExistForCell = waveGraph.GetNode(neighborCell, out var neighborNode);
                    var doesNodeKnowNeighbor = node.GetNeighborAtDirection(direction, out _);

                    if (!doesNodeExistForCell)
                    {
                        if (cellsFrontier.Count >= cellCount)
                            continue; // TODO should be break?

                        neighborNode = new Node<Cell>(
                            waveGraph.NodeCardinality,
                            neighborCell
                        );
                        waveGraph.AddNode(neighborNode);
                    }

                    if (!doesNodeExistForCell || !doesNodeKnowNeighbor)
                    {
                        node.RegisterNeighbor(neighborNode, direction);
                        neighborNode.RegisterNeighbor(node, waveGraph.GetOppositeDirection(direction));
                    }

                    var willVisitNeighbor = cellsFrontier.Add(neighborCell)
                                            && (
                                                cellsFrontier.Count < cellCount
                                                || !doesNodeExistForCell
                                            );
                    if (willVisitNeighbor)
                        nodesQueue.Enqueue(neighborNode);

                    if (!node.Content.IsTotalSuperposition && neighborNode.Content.IsTotalSuperposition)
                    {
                        MatchSelfToNeighbor(waveGraph, neighborNode, tileData,
                            waveGraph.GetOppositeDirection(direction));
                        if (neighborNode.Content.Count <= 1) yield return neighborCell;
                    }

                    if (doesNodeExistForCell && !neighborNode.Content.IsTotalSuperposition && !neighborNode.Content.IsFailed)
                    {
                        MatchSelfToNeighbor(waveGraph, node, tileData, direction);
                        if (node.Content.Count <= 1) yield return node.Content;
                    }
                }
            }
        }

        private static bool Observe(Graph<Cell> waveGraph, out Node<Cell> node)
        {
            return FindLowestEntropyNode(waveGraph, out node);
        }

        private static void Collapse(Cell cell, Random random, Dictionary<int, float> probabilityLookup)
        {
            var tile = PickTile(cell, random, probabilityLookup);
            cell.Clear();
            if (cell.Count >= 0)
                cell.Add(tile);
        }

        public static void MatchSelfToNeighbor(
            Graph<Cell> waveGraph,
            Node<Cell> node,
            TileData[] tileData,
            int direction
        )
        {
            if (!node.GetNeighborAtDirection(direction, out var neighborNode))
                return;

            var constrainedCell = new HashSet<int>();
            var oppositeDirection = waveGraph.GetOppositeDirection(direction);

            foreach (var neighborTile in neighborNode.Content)
                constrainedCell.UnionWith(tileData[neighborTile].ConnectionsPerDirection[oppositeDirection]);

            node.Content.IntersectWith(constrainedCell);
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

        private static IEnumerable<Cell> Propagate(
            IReadOnlyList<TileData> tileData,
            Node<Cell> seedNode
        )
        {
            var neighboringNodes = new Stack<Node<Cell>>();
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

                    if (neighborCell.IsFailed) continue;

                    var isSuperpositionNew = MatchNeighbor(
                        neighborCell,
                        cell.Select(t => tileData[t].ConnectionsPerDirection[direction]),
                        out var superposition
                    );

                    if (!isSuperpositionNew)
                        continue;

                    neighborNode.Content = new Cell(superposition, tileData.Count, neighborNode.Content.PhysicalPosition);
                    neighboringNodes.Push(neighborNode);

                    if (superposition.Count <= 1)
                        yield return neighborNode.Content;
                }
            }
        }

        private static bool FindLowestEntropyNode(
            Graph<Cell> waveGraph,
            out Node<Cell> lowestEntropyNode
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

        private static int PickTile(Cell cell, Random random, Dictionary<int, float> probabilityLookup)
        {
            if (cell.Count == 1) return cell.ElementAt(0);
            if (cell.Count == 0) return -1;

            var randomEvent = random.NextDouble();
            foreach (var tile in cell)
            {
                if (!probabilityLookup.TryGetValue(tile, out var probability))
                    continue;

                probability /= cell.Count;
                if ((randomEvent -= probability) <= 0) return tile;
            }

            var randIndex = random.Next(0, cell.Count);
            return cell.ElementAt(randIndex);
        }

        public static IEnumerable<Cell> ParseAll(
            Graph<Cell> waveGrid,
            IWaveFunctionInput input
        )
        {
            return waveGrid.Select(node => node.Content);
        }

        // private static Cell ParseCell(Node<Cell> node, IReadOnlyList<TileData> tileData)
        // {
        //     return (node.Content.Count == 1 ? tileData[node.Content.ElementAt(0)] : null, node.Coordinates);
        // }

        public static IEnumerable<Cell> Execute(
            Graph<Cell> waveGraph,
            Random random,
            IWaveFunctionInput input,
            Node<Cell> collapseNode
        )
        {
            var iterations = 0;

            do
            {
                Collapse(collapseNode.Content, random, input.ProbabilityLookup);
                yield return collapseNode.Content;

                foreach (var parsedCell in Propagate(input.TileData, collapseNode))
                    yield return parsedCell;

                if (iterations++ <= 1000) continue;
                Debug.LogWarning($"[WaveFunctionCollapse] WFC loop iterated the maximum number of iterations.");
                break;
            } while (Observe(waveGraph, out collapseNode));
        }

        public static IEnumerable<Cell> Execute(
            Graph<Cell> waveGraph,
            Random random,
            IWaveFunctionInput input
        )
        {
            var startCollapseNode = waveGraph.GetRandomNode();
            return Execute(waveGraph, random, input, startCollapseNode);
        }

        public static IEnumerable<Cell> Generate(
            IWaveFunctionInput input,
            WaveFunctionCollapseOptions options
        )
        {
            var random = new Random(options.Seed);

            var waveGraph = InitializeWaveGraph(input, options);

            var seedCell = new Cell(input.TileData.Length, new Vector2());

            AddCells(
                waveGraph,
                seedCell,
                100,
                input.TileData,
                input.NeighborOffsets
            );

            Execute(waveGraph, random, input);

            return ParseAll(waveGraph, input);
        }
    }
}