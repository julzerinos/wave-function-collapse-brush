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
        public static Graph<Cell, CellCoordinates> InitializeWaveGraph(
            IWaveFunctionInput input,
            WaveFunctionCollapseOptions options
        )
        {
            var waveGraph = new Graph<Cell, CellCoordinates>(
                input.GetOppositeDirectionIndex,
                input.Cardinality,
                () => Cell.Factory(input.TileCount),
                input.NeighborOffsets
            );

            return waveGraph;
        }

        public static IEnumerable<(TileData, CellCoordinates)> AddCells(
            Graph<Cell, CellCoordinates> waveGraph,
            CellCoordinates seedPosition,
            int cellCount,
            TileData[] tileData,
            bool overwrite = false
        )
        {
            if (!waveGraph.GetNode(seedPosition, out var seedNode))
            {
                seedNode = new Node<Cell, CellCoordinates>(waveGraph.NodeCardinality, waveGraph.ContentFactory(), seedPosition);
                waveGraph.AddNode(seedNode);
            }

            var positionFrontier = new HashSet<CellCoordinates> { seedNode.Coordinates };
            var nodesQueue = new Queue<Node<Cell, CellCoordinates>>();
            nodesQueue.Enqueue(seedNode);

            while (nodesQueue.Count > 0)
            {
                var node = nodesQueue.Dequeue();

                // TODO fix overwrite
                if (overwrite)
                    node.Content = waveGraph.ContentFactory();

                foreach (
                    var (neighborOffset, direction)
                    in
                    waveGraph.NeighborOffsetsGenerator.Select((n, i) => (n, i))
                )
                {
                    var neighborPosition = node.Coordinates + neighborOffset;

                    var doesNodeKnowNeighbor = node.GetNeighborAtDirection(direction, out var neighborNode);
                    var doesNodeExistAtPosition = waveGraph.GetNode(neighborPosition, out neighborNode);
                    if (!doesNodeExistAtPosition)
                    {
                        if (positionFrontier.Count >= cellCount)
                            continue;

                        neighborNode = new Node<Cell, CellCoordinates>(
                            waveGraph.NodeCardinality,
                            waveGraph.ContentFactory(),
                            neighborPosition
                        );
                        waveGraph.AddNode(neighborNode);
                    }

                    if ((!doesNodeKnowNeighbor && doesNodeExistAtPosition) || !doesNodeExistAtPosition)
                    {
                        node.RegisterNeighbor(neighborNode, direction);
                        neighborNode.RegisterNeighbor(node, waveGraph.GetOppositeDirection(direction));
                    }

                    var willVisitNeighbor = positionFrontier.Add(neighborPosition) &&
                                            (positionFrontier.Count < cellCount || !doesNodeExistAtPosition);
                    if (willVisitNeighbor)
                        nodesQueue.Enqueue(neighborNode);

                    if (!node.Content.IsTotalSuperposition && neighborNode.Content.IsTotalSuperposition)
                    {
                        MatchSelfToNeighbor(waveGraph, neighborNode, tileData, waveGraph.GetOppositeDirection(direction));
                        if (neighborNode.Content.Count <= 1) yield return ParseCell(neighborNode, tileData);
                    }

                    if (overwrite && willVisitNeighbor) continue;

                    if (doesNodeExistAtPosition && !neighborNode.Content.IsTotalSuperposition)
                    {
                        MatchSelfToNeighbor(waveGraph, node, tileData, direction);
                        if (node.Content.Count <= 1) yield return ParseCell(neighborNode, tileData);
                    }
                }
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

        public static void MatchSelfToNeighbor(
            Graph<Cell, CellCoordinates> waveGraph,
            Node<Cell, CellCoordinates> node,
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

        private static IEnumerable<(TileData, CellCoordinates)> Propagate(
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

                    neighborNode.Content = new Cell(superposition, tileData.Count);
                    neighboringNodes.Push(neighborNode);

                    if (superposition.Count <= 1)
                        yield return ParseCell(neighborNode, tileData);
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

        public static IEnumerable<(TileData, CellCoordinates)> ParseAll(
            Graph<Cell, CellCoordinates> waveGrid,
            IWaveFunctionInput input
        )
        {
            return waveGrid.Select(node => ParseCell(node, input.TileData));
        }

        private static (TileData, CellCoordinates) ParseCell(Node<Cell, CellCoordinates> node, IReadOnlyList<TileData> tileData)
        {
            return (node.Content.Count == 1 ? tileData[node.Content.ElementAt(0)] : null, node.Coordinates);
        }

        public static IEnumerable<(TileData, CellCoordinates)> Execute(
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
                yield return ParseCell(collapseNode, input.TileData);

                foreach (var parsedCell in Propagate(input.TileData, collapseNode))
                    yield return parsedCell;

                if (iterations++ <= 1000) continue;
                Debug.LogWarning($"[WaveFunctionCollapse] WFC loop iterated the maximum number of iterations.");
                break;
            } while (Observe(waveGraph, out collapseNode));
        }

        public static IEnumerable<(TileData, CellCoordinates)> Execute(
            Graph<Cell, CellCoordinates> waveGraph,
            Random random,
            IWaveFunctionInput input
        )
        {
            var startCollapseNode = waveGraph.GetRandomNode();
            return Execute(waveGraph, random, input, startCollapseNode);
        }

        public static IEnumerable<(TileData, CellCoordinates)> Generate(IWaveFunctionInput input, WaveFunctionCollapseOptions options)
        {
            var random = new Random(options.Seed);

            var waveGraph = InitializeWaveGraph(input, options);

            AddCells(
                waveGraph,
                new CellCoordinates(0, 0),
                100,
                input.TileData
            );

            Execute(waveGraph, random, input);

            return ParseAll(waveGraph, input);
        }
    }
}