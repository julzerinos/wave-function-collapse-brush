using System.Collections.Generic;
using System.Linq;
using Algorithms.Tilesets;
using Algorithms.WaveFunctionCollapse.Input;
using UnityEngine;
using Utility.Graph;
using Random = System.Random;

namespace Algorithms.WaveFunctionCollapse
{
    public static class WaveFunctionCollapse
    {
        public static Graph<Cell> InitializeWaveGraph(int cardinality, int tileCount)
        {
            var waveGraph = new Graph<Cell>(GetOppositeDirectionIndex);

            var startCellNode = new Node<Cell>(cardinality, Cell.Factory((0, 0), tileCount));
            RecursivelyAddNode(startCellNode);

            return waveGraph;

            void RecursivelyAddNode(Node<Cell> node)
            {
                waveGraph.AddNode(node);

                for (var directionIndex = 0; directionIndex < cardinality; directionIndex++)
                {
                    if (!GetNeighborPosition(node.Content.Coordinates, directionIndex, out var neighborPosition))
                        continue;

                    var doesNodeExist = waveGraph.GetNode(neighborPosition.GetHashCode(), out var neighborNode);

                    if (!doesNodeExist)
                        neighborNode = new Node<Cell>(4, Cell.Factory(neighborPosition, tileCount));

                    node.RegisterNeighbor(neighborNode, directionIndex);
                    neighborNode.RegisterNeighbor(node, GetOppositeDirectionIndex(directionIndex));

                    if (!doesNodeExist)
                        RecursivelyAddNode(neighborNode);
                }
            }

            // TODO from input
            int GetOppositeDirectionIndex(int direction) => (direction + cardinality / 2) % cardinality;

            // TODO from input
            bool GetNeighborPosition(
                (float x, float y) centerPosition,
                int directionIndex,
                out (float x, float y) neighborPosition
            )
            {
                // TODO should come from input
                var neighbors = new[]
                {
                    (x: 0f, y: 1f),
                    (x: 1f, y: 0f),
                    (x: 0f, y: -1f),
                    (x: -1f, y: 0f)
                };

                neighborPosition = (float.MaxValue, float.MaxValue);

                neighborPosition = (centerPosition.x + neighbors[directionIndex].x, centerPosition.y + neighbors[directionIndex].y);
                // TODO change to actual constraint checks
                return neighborPosition.x is < 25 and >= 0 && neighborPosition.y is < 25 and >= 0;
            }
        }

        private static bool Observe(Graph<Cell> waveGraph, out Node<Cell> node)
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
            Graph<Cell> waveGraph,
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

                if (!cell.Any()) // TODO is the logic true
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

                    neighborNode.Content = new Cell(superposition, neighborCell.Coordinates);
                    neighboringNodes.Push(neighborNode);
                    // TODO do I need a set for tracking nodes I've been to?
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

        private static int PickTile(Cell cell, Random random)
        {
            if (cell.Count == 1) return cell.ElementAt(0);

            // TODO add tile probability distribution
            var randIndex = random.Next(0, cell.Count);
            return cell.ElementAt(randIndex);
        }

        public static IEnumerable<(TileData, (float x, float y))> Parse(
            Graph<Cell> waveGrid,
            IWaveFunctionInput input
        )
        {
            foreach (var node in waveGrid)
            {
                var cell = node.Content;
                var tile = cell.Count == 1 ? input.TileData[cell.ElementAt(0)] : null;

                yield return (tile, cell.Coordinates);
            }
        }

        public static void Execute(
            Graph<Cell> waveGraph,
            Random random,
            IWaveFunctionInput input,
            Node<Cell> collapseNode
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
            Graph<Cell> waveGraph,
            Random random,
            IWaveFunctionInput input
        )
        {
            var startCollapseNode = waveGraph.GetRandomNode();
            Execute(waveGraph, random, input, startCollapseNode);
        }

        public static IEnumerable<(TileData, (float x, float y))> Generate(IWaveFunctionInput input, WaveFunctionCollapseOptions options)
        {
            var random = new Random(options.Seed);

            // TODO update cardinality from input
            var waveGraph = InitializeWaveGraph(4, input.TileCount);

            Execute(waveGraph, random, input);

            return Parse(waveGraph, input);
        }
    }
}