using System;
using System.Collections.Generic;
using Algorithms.Tilesets;
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

        public void SetActiveTile(TileData data)
        {
            if (data.OriginalIndex != ActiveTileIndex)
            {
                if (ActiveTileIndex >= 0)
                    _possibleTiles[ActiveTileIndex].SetActive(false);

                _possibleTiles[data.OriginalIndex].SetActive(true);
                ActiveTileIndex = data.OriginalIndex;
            }

            if (data.Transformation.DegreesRotation.Equals(Transformation.DegreesRotation))
                return;

            transform.rotation = Quaternion.Euler(0, data.Transformation.DegreesRotation, 0);
            Transformation = data.Transformation;
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