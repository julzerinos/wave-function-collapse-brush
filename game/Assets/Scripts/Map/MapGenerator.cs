using System;
using System.Collections.Generic;
using System.Linq;
using Algorithms.Tilesets;
using Algorithms.WaveFunctionCollapse;
using Algorithms.WaveFunctionCollapse.Input;
using Algorithms.WaveFunctionCollapse.WaveGraph;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;
using Utility.Graph;


namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private string tileSetName;
        [SerializeField] private WaveFunctionCollapseOptions options;

        [SerializeField] private Transform brush;
        [SerializeField] private GameObject invalidTile;

        private WaveFunctionCollapseComputer _computer;
        private GameObject[] _tilePrefabs;
        private readonly Dictionary<Cell, Tile> _instantiatedTilesLookup = new();

        private Camera _camera;
        private Vector2Int _lastHitPoint = new(0, 0);

        private void Awake()
        {
            _camera = Camera.main;

            var tileSetPath = $"Models/Tiles/{tileSetName}";

            var waveFunctionInputFromJson = new WaveFunctionInputFromTypesJson($"{tileSetPath}/configuration");
            _computer = new WaveFunctionCollapseComputer(waveFunctionInputFromJson, options);

            _tilePrefabs = Resources.LoadAll<GameObject>(tileSetPath)
                .Where(r => waveFunctionInputFromJson.Tiles.Contains(r.name))
                .ToArray();

            BuildMap(
                _computer.Expand(
                    new Cell(waveFunctionInputFromJson.TileData.Length, new Vector2()),
                    options.initialPatchCount
                )
            );
        }

        // private void Update()
        // {
        //     var ray = _camera.ScreenPointToRay(Input.mousePosition);
        //     if (!Physics.Raycast(ray, out var hit))
        //         return;
        //
        //     brush.position = hit.point;
        //
        //     if (!Input.GetMouseButton(0)) return;
        //
        //     var hitPointFlat = new Vector2Int(Mathf.RoundToInt(hit.point.x), Mathf.RoundToInt(hit.point.z)) /
        //                        (int)options.tileOffset;
        //     if (hitPointFlat.Equals(_lastHitPoint))
        //         return;
        //
        //     _lastHitPoint = hitPointFlat;
        //     DrawPatch();
        // }
        //
        // private void DrawPatch()
        // {
        //     BuildMap(
        //         _computer.Expand(_lastHitPoint, options.patchCellCount, options.overwritePatch)
        //     );
        //     BuildMap(_computer.CompleteGrid());
        // }

        private void BuildMap(IEnumerable<Cell> parsedCells)
        {
            foreach (var cell in parsedCells)
            {
                if (!_instantiatedTilesLookup.TryGetValue(cell, out var tile))
                {
                    var tileGameObject = new GameObject($"Tile {cell.PhysicalPosition}")
                    {
                        transform =
                        {
                            parent = transform,
                            position = new Vector3(cell.PhysicalPosition.x, 0, cell.PhysicalPosition.y) * options.tileOffset
                        }
                    };
                    tile = tileGameObject.AddComponent<Tile>();
                    foreach (var tilePrefab in _tilePrefabs)
                        tile.AddTile(tilePrefab);
                    tile.AddTile(invalidTile);
                    _instantiatedTilesLookup[cell] = tile;
                }

                if (cell.IsFailed)
                {
                    Debug.LogWarning($"[MapGenerator] No tile found for col-row position {cell.PhysicalPosition}.");
                    tile.SetTileInvalid();
                    continue;
                }

                // if (tileData.OriginalIndex < 0 || tileData.OriginalIndex >= _tilePrefabs.Length)
                // {
                //     Debug.LogError(
                //         $"[MapGenerator] Could not find tile model for index {tileData.OriginalIndex} defined in configuration (skipping).");
                //     tile.SetTileInvalid();
                //     continue;
                // }
                //
                // if (tile.Transformation.DegreesRotation.Equals(tileData.Transformation.DegreesRotation) &&
                //     tile.ActiveTileIndex == tileData.OriginalIndex)
                //     continue;

                tile.SetActiveTile(cell);
            }
        }
    }
}