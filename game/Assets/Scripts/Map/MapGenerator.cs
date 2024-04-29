using System.Collections.Generic;
using Algorithms.Tilesets;
using Algorithms.WaveFunctionCollapse;
using Algorithms.WaveFunctionCollapse.Input;
using Algorithms.WaveFunctionCollapse.WaveGraph;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;


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
        private readonly Dictionary<CellCoordinates, Tile> _instantiatedTilesLookup = new();

        private Camera _camera;
        private Vector2Int _lastHitPoint = new(0, 0);

        private void Awake()
        {
            _camera = Camera.main;

            var tileSetPath = $"Models/Tiles/{tileSetName}";
            _tilePrefabs = Resources.LoadAll<GameObject>(tileSetPath);

            var waveFunctionInputFromJson = new WaveFunctionInputFromTypesJson($"{tileSetPath}/configuration");
            _computer = new WaveFunctionCollapseComputer(waveFunctionInputFromJson, options);

            if (options.initialPatchCount > 0)
                BuildMap(_computer.Expand(new CellCoordinates(options.initialPatchLocation), options.initialPatchCount));
        }

        private void Update()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit))
                return;

            brush.position = hit.point;

            if (!Input.GetMouseButton(0)) return;

            var hitPointFlat = new Vector2Int(Mathf.RoundToInt(hit.point.x), Mathf.RoundToInt(hit.point.z));
            if (hitPointFlat.Equals(_lastHitPoint))
                return;

            _lastHitPoint = hitPointFlat;
            DrawPatch();
        }

        private void DrawPatch()
        {
            BuildMap(
                _computer.Expand(new CellCoordinates(_lastHitPoint), options.patchCellCount, options.overwritePatch)
            );
            BuildMap(_computer.CompleteGrid());
        }

        private void BuildMap(IEnumerable<(TileData, CellCoordinates)> parsedCells)
        {
            foreach (var (tileData, cellCoordinates) in parsedCells)
            {
                if (!_instantiatedTilesLookup.TryGetValue(cellCoordinates, out var tile))
                {
                    var tileGameObject = new GameObject($"Tile {cellCoordinates}")
                    {
                        transform =
                        {
                            parent = transform,
                            position = new Vector3(cellCoordinates.X, 0, cellCoordinates.Y) * options.tileOffset
                        }
                    };
                    tile = tileGameObject.AddComponent<Tile>();
                    foreach (var tilePrefab in _tilePrefabs)
                        tile.AddTile(tilePrefab);
                    tile.AddTile(invalidTile);
                    _instantiatedTilesLookup[cellCoordinates] = tile;
                }

                if (tileData == null)
                {
                    Debug.LogWarning($"[MapGenerator] No tile found for col-row position {cellCoordinates}.");
                    tile.SetTileInvalid();
                    continue;
                }

                if (tileData.OriginalIndex < 0 || tileData.OriginalIndex >= _tilePrefabs.Length)
                {
                    Debug.LogError(
                        $"[MapGenerator] Could not find tile model for index {tileData.OriginalIndex} defined in configuration (skipping).");
                    tile.SetTileInvalid();
                    continue;
                }

                if (tile.Transformation == tileData.Transformation && tile.ActiveTileIndex == tileData.OriginalIndex)
                    continue;

                tile.SetActiveTile(tileData);
            }
        }
    }
}