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

        // TODO update cardinality
        public WaveFunctionCollapseComputer(IWaveFunctionInput input, WaveFunctionCollapseOptions options)
        {
            _input = input;
            _options = options;
            _random = new Random(_options.Seed);
            _waveGraph = WaveFunctionCollapse.InitializeWaveGraph(input, options);
        }

        public void CompleteGrid()
        {
            WaveFunctionCollapse.Execute(_waveGraph, _random, _input);
        }

        public void UnCollapseCells(CellCoordinates patchCenter, int maxCells)
        {
            if (!_waveGraph.GetNode(patchCenter, out var startNode))
            {
                Debug.LogError($"[WFCComputer > UnCollapseCells] Could not find node for position {patchCenter}.");
                return;
            }

            var neighboringNodes = new Queue<Node<Cell>>();
            neighboringNodes.Enqueue(startNode);

            var nodesToFix = new Stack<Node<Cell>>();
            var nodesExplored = new HashSet<Node<Cell>>();

            var cellsReCollapsed = 0;

            while (neighboringNodes.Count > 0 && cellsReCollapsed < maxCells)
            {
                var node = neighboringNodes.Dequeue();

                nodesToFix.Push(node);
                nodesExplored.Add(node);
                node.Content = Cell.Factory(_input.TileCount);
                cellsReCollapsed++;

                foreach (var (neighbor, _) in node.Neighbors)
                    if (!nodesExplored.Contains(neighbor)) // TODO required?
                        neighboringNodes.Enqueue(neighbor);
            }

            while (nodesToFix.Count > 0)
            {
                var node = nodesToFix.Pop();
                var cell = node.Content;

                if (cell.Count == 0)
                {
                    Debug.LogWarning(
                        "[WaveFunctionCollapse > Propagate] Wave collapse encountered failed superposition (skipping)."
                    );
                    continue;
                }

                foreach (var (neighborNode, direction) in node.Neighbors)
                {
                    var constrainedCell = new HashSet<int>();
                    var oppositeDirection = _waveGraph.GetOppositeDirection(direction);

                    if (!nodesExplored.Contains(neighborNode))
                        foreach (var neighborTile in neighborNode.Content)
                            constrainedCell.UnionWith(_input.TileData[neighborTile].ConnectionsPerDirection[oppositeDirection]);

                    cell.IntersectWith(constrainedCell);
                }
            }
        }

        public void Clear()
        {
            // TODO update cardinality
            _waveGraph = WaveFunctionCollapse.InitializeWaveGraph(_input, _options);
            _random = new Random(_options.Seed);
        }

        public IEnumerable<(TileData, CellCoordinates)> ParseResult()
        {
            return WaveFunctionCollapse.Parse(_waveGraph, _input);
        }
    }
}