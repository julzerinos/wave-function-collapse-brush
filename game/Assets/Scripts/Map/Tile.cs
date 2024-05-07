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

        private float _currentRotation;
        private float[] _rotations;

        public void SetRotation(float[] rotations)
        {
            _rotations = rotations;
        }

        public void AddTile(GameObject tile)
        {
            var tileCopy = Instantiate(tile, transform);
            _possibleTiles.Add(tileCopy);
            tileCopy.SetActive(false);
        }

        public void SetActiveTile(Cell cell)
        {
            var transformedIndex = cell.ElementAt(0);

            var trueIndex = transformedIndex / _rotations.Length;
            if (trueIndex != ActiveTileIndex)
            {
                if (ActiveTileIndex >= 0)
                    _possibleTiles[ActiveTileIndex].SetActive(false);

                _possibleTiles[trueIndex].SetActive(true);
                ActiveTileIndex = trueIndex;
            }

            var newRotation = _rotations[transformedIndex % _rotations.Length];
            if (_currentRotation.Equals(newRotation))
                return;

            transform.rotation = Quaternion.Euler(0, newRotation, 0);
            _currentRotation = newRotation;
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