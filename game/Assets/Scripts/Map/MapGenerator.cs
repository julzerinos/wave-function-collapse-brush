using Algorithms.Tilesets;
using Algorithms.WaveFunctionCollapse;
using Algorithms.WaveFunctionCollapse.Input;
using UnityEngine;


namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private string tileSetName;
        [SerializeField] private WaveFunctionCollapseOptions options;

        [SerializeField] private Vector2Int patchCenter;
        [SerializeField] private int patchCellCount = 50;

        private WaveFunctionCollapseComputer _computer;
        private GameObject[] _tilePrefabs;

        private void Awake()
        {
            var tileSetPath = $"Models/Tiles/{tileSetName}";
            _tilePrefabs = Resources.LoadAll<GameObject>(tileSetPath);

            var waveFunctionInputFromJson = new WaveFunctionInputFromTypesJson($"{tileSetPath}/configuration");
            _computer = new WaveFunctionCollapseComputer(waveFunctionInputFromJson, options);
            _computer.CompleteGrid();

            BuildMap();
        }

        public void RegeneratePatch()
        {
            _computer.UnCollapseCells((patchCenter.x, patchCenter.y), patchCellCount);
            _computer.CompleteGrid();

            BuildMap();
        }

        public void RegenerateMap()
        {
            _computer.Clear();
            _computer.CompleteGrid();
            BuildMap();
        }

        private void BuildMap()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            foreach (var (tileData, (col, row)) in _computer.ParseResult())
            {
                if (tileData == null)
                {
                    Debug.LogWarning($"[MapGenerator] No tile found for col-row position {(col, row)} (skipping).");
                    continue;
                }

                if (tileData.OriginalIndex < 0 || tileData.OriginalIndex >= _tilePrefabs.Length)
                {
                    Debug.LogError(
                        $"[MapGenerator] Could not find tile model for index {tileData.OriginalIndex} defined in configuration (skipping).");
                    continue;
                }

                var position = new Vector3(col, 0, row) * options.tileOffset;

                var tileGameObject = Instantiate(_tilePrefabs[tileData.OriginalIndex], transform, true);
                tileGameObject.name = $"{tileGameObject.name.Replace("(Clone)", "")}.{tileData.Transformation} {position}";
                tileGameObject.transform.position = position;

                switch (tileData.Transformation)
                {
                    case TileTransformation.Rotate90:
                        tileGameObject.transform.Rotate(Vector3.up, 90);
                        break;
                    case TileTransformation.Rotate180:
                        tileGameObject.transform.Rotate(Vector3.up, 180);
                        break;
                    case TileTransformation.Rotate270:
                        tileGameObject.transform.Rotate(Vector3.up, 270);
                        break;
                    case TileTransformation.Original:
                    default:
                        break;
                }
            }
        }

        [InspectorButton("RegenerateMap")] public bool regenerateMap;

        [InspectorButton("RegeneratePatch")] public bool regeneratePatch;
    }
}