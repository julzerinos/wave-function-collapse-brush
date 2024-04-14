using System.Linq;
using Algorithms.WaveFunctionCollapse;
using UnityEngine;

namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private string tileSetName;
        [SerializeField] private WaveFunctionCollapseOptions options;

        private WaveFunctionCollapseComputer _computer;

        private void Awake()
        {
            var tileSetPath = $"Models/Tiles/{tileSetName}";
            var tileByNameLookup = Resources.LoadAll<GameObject>(tileSetPath).ToDictionary(tile => tile.name);

            var waveFunctionInputFromJson = new WaveFunctionInputFromJson($"{tileSetPath}/configuration");
            _computer = new WaveFunctionCollapseComputer(waveFunctionInputFromJson, options);

            _computer.CompleteGrid();
            var waveFunctionResult = _computer.ParseResult();

            for (var col = 0; col < options.gridSize; col++)
            for (var row = 0; row < options.gridSize; row++)
            {
                var tileName = waveFunctionResult.TileGrid[col, row];

                if (tileName.Length == 0)
                {
                    Debug.LogWarning($"[MapGenerator] No tile found for col-row position {(col, row)} (skipping).");
                    continue;
                }

                if (!tileByNameLookup.TryGetValue(tileName, out var tilePrefab))
                {
                    Debug.LogError(
                        $"[MapGenerator] Could not find tile model for name {tileName} defined in configuration (skipping).");
                    continue;
                }

                var position = new Vector3(col, 0, row) * options.tileOffset;

                var tileGameObject = Instantiate(tilePrefab, transform, true);
                tileGameObject.name = $"{tileGameObject.name} {position}";
                tileGameObject.transform.position = position;
            }
        }
    }
}