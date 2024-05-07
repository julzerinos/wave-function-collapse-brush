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
        private readonly Dictionary<Collider, Cell> _cellByTileLookup = new();

        private IWaveFunctionInput _input;

        private Camera _camera;
        private Cell _lastHitCell = null;
        private Vector3 _brushTarget;

        private void Awake()
        {
            _camera = Camera.main;

            var tileSetPath = $"Models/Tiles/{tileSetName}";

            _input = new WaveFunctionInputFromTypesJson($"{tileSetPath}/configuration");
            _computer = new WaveFunctionCollapseComputer(_input, options);

            _tilePrefabs = Resources.LoadAll<GameObject>(tileSetPath)
                .Where(r => _input.Tiles.Contains(r.name))
                .ToArray();

            BuildMap(
                _computer.Expand(
                    new Cell(_input.TileData.Length, new Vector2()),
                    options.initialPatchCount
                )
            );
        }

        private void Update()
        {
            brush.position = Vector3.Lerp(brush.position, _brushTarget, .25f);

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (
                Physics.Raycast(ray, out var hit)
                && _cellByTileLookup.TryGetValue(hit.collider, out var cell)
                && !cell.Equals(_lastHitCell)
            )
            {
                _brushTarget = hit.transform.position;
                _lastHitCell = cell;
                return;
            }

            if (Input.GetMouseButton(0))
                DrawPatch();
        }

        private void DrawPatch()
        {
            if (_lastHitCell is null) return;

            BuildMap(
                _computer.Expand(_lastHitCell, options.patchCellCount, options.overwritePatch)
            );
        }

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

                    var tileCollider = tileGameObject.AddComponent<SphereCollider>();
                    tileCollider.radius = options.tileOffset;
                    _cellByTileLookup.Add(tileCollider, cell);

                    tile = tileGameObject.AddComponent<Tile>();
                    foreach (var tilePrefab in _tilePrefabs)
                        tile.AddTile(tilePrefab);
                    tile.AddTile(invalidTile);
                    tile.SetRotation(_input.Rotations);

                    _instantiatedTilesLookup[cell] = tile;
                }

                if (cell.IsFailed)
                {
                    Debug.LogWarning($"[MapGenerator] No tile found for col-row position {cell.PhysicalPosition}.");
                    tile.SetTileInvalid();
                    continue;
                }

                tile.SetActiveTile(cell);
            }
        }
    }
}