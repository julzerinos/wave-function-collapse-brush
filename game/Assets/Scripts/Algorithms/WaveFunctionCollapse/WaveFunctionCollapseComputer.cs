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
        private Graph<Cell, CellCoordinates> _waveGraph;

        public WaveFunctionCollapseComputer(IWaveFunctionInput input, WaveFunctionCollapseOptions options)
        {
            _input = input;
            _options = options;
            _random = new Random(_options.Seed);
            _waveGraph = WaveFunctionCollapse.InitializeWaveGraph(input, options);
            WaveFunctionCollapse.AddCells(_waveGraph, new CellCoordinates(0, 0), options.defaultPatchCellCount, _input.TileData);
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

            var neighboringNodes = new Queue<Node<Cell, CellCoordinates>>();
            neighboringNodes.Enqueue(startNode);

            var nodesToFix = new HashSet<Node<Cell, CellCoordinates>>();
            var nodesExplored = new HashSet<Node<Cell, CellCoordinates>>();

            startNode.Content = Cell.Factory(_input.TileCount);
            nodesToFix.Add(startNode);
            nodesExplored.Add(startNode);

            var cellsReCollapsed = 0;
            while (neighboringNodes.Count > 0)
            {
                var node = neighboringNodes.Dequeue();

                var didEncounterMaxCells = true;
                foreach (var (neighbor, _) in node.Neighbors)
                {
                    if (nodesExplored.Contains(neighbor))
                        continue;

                    neighbor.Content = Cell.Factory(_input.TileCount);
                    nodesToFix.Add(neighbor);
                    nodesExplored.Add(neighbor);

                    if (++cellsReCollapsed > maxCells)
                    {
                        didEncounterMaxCells = false;
                        break;
                    }

                    neighboringNodes.Enqueue(neighbor);
                }

                if (!didEncounterMaxCells) break;

                nodesToFix.Remove(node);
            }

            while (nodesToFix.Count > 0)
            {
                var node = nodesToFix.ElementAt(0);
                nodesToFix.Remove(node);
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

                    foreach (var neighborTile in neighborNode.Content)
                        constrainedCell.UnionWith(_input.TileData[neighborTile].ConnectionsPerDirection[oppositeDirection]);

                    cell.IntersectWith(constrainedCell);
                }
            }
        }

        public void Expand(CellCoordinates patchCenter, int cellCount)
        {
            WaveFunctionCollapse.AddCells(_waveGraph, patchCenter, cellCount, _input.TileData);
        }

        public void Clear()
        {
            _waveGraph = WaveFunctionCollapse.InitializeWaveGraph(_input, _options);
            WaveFunctionCollapse.AddCells(_waveGraph, new CellCoordinates(0, 0), _options.defaultPatchCellCount, _input.TileData);
            _random = new Random(_options.Seed);
        }

        public IEnumerable<(TileData, CellCoordinates)> ParseResult()
        {
            return WaveFunctionCollapse.Parse(_waveGraph, _input);
        }
    }
}