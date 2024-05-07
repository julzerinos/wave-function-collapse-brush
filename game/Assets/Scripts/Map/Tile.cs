using System;
using System.Collections.Generic;
using System.Linq;
using Algorithms.Tilesets;
using Algorithms.WaveFunctionCollapse.WaveGraph;
using UnityEngine;

namespace Map
{
    public class Tile : MonoBehaviour
    {
        private readonly List<GameObject> _possibleTiles = new();

        public int ActiveTileIndex { get; private set; } = -1;
        public TileTransformation Transformation { get; private set; }

        public void AddTile(GameObject tile)
        {
            var tileCopy = Instantiate(tile, transform);
            _possibleTiles.Add(tileCopy);
            tileCopy.SetActive(false);
        }

        public void SetActiveTile(Cell cell)
        {
            // TODO doesn't work for transformed tiles (with different index)
            var index = cell.ElementAt(0);

            if (index != ActiveTileIndex)
            {
                if (ActiveTileIndex >= 0)
                    _possibleTiles[ActiveTileIndex].SetActive(false);

                _possibleTiles[index].SetActive(true);
                ActiveTileIndex = index;
            }

            // if (data.Transformation.DegreesRotation.Equals(Transformation.DegreesRotation))
            //     return;
            //
            // transform.rotation = Quaternion.Euler(0, data.Transformation.DegreesRotation, 0);
            // Transformation = data.Transformation;
        }

        public void SetTileInactive()
        {
            if (ActiveTileIndex < 0) return;

            _possibleTiles[ActiveTileIndex].SetActive(false);
            ActiveTileIndex = -1;
        }

        public void SetTileInvalid()
        {
            if (ActiveTileIndex > 0)
                _possibleTiles[ActiveTileIndex].SetActive(false);
            ActiveTileIndex = _possibleTiles.Count - 1;

            _possibleTiles[^1].SetActive(true);
        }
    }
}