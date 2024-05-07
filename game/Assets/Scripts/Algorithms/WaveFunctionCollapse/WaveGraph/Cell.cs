using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Algorithms.WaveFunctionCollapse.WaveGraph
{
    public class Cell : HashSet<int>
    {
        private readonly int _maxTileCount;

        public Vector2 PhysicalPosition { get; }

        public Cell(IEnumerable<int> collection, int maxTileCount, Vector2 physicalPosition) : base(collection)
        {
            _maxTileCount = maxTileCount;
            PhysicalPosition = physicalPosition;
        }

        public Cell(int maxTileCount, Vector2 physicalPosition)
            : this(Enumerable.Range(0, maxTileCount), maxTileCount, physicalPosition) { }

        public override string ToString()
        {
            return $"Cell of tiles {{ {string.Join(',', this)} }}.";
        }
        
        public bool IsTotalSuperposition => Count == _maxTileCount;
        public bool IsDetermined => Count == 1;
        public bool IsFailed => Count == 0;

        public override int GetHashCode()
        {
            return PhysicalPosition.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Cell cell && cell.PhysicalPosition.Equals(PhysicalPosition);
        }
    }
}