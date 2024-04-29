using System.Collections.Generic;
using System.Linq;
using Utility.Graph;

namespace Algorithms.WaveFunctionCollapse.WaveGraph
{
    public class Cell : HashSet<int>
    {
        private int _maxTileCount;

        public Cell(IEnumerable<int> collection, int maxTileCount) : base(collection)
        {
            _maxTileCount = maxTileCount;
        }

        public override string ToString()
        {
            return $"Cell of tiles {{ {string.Join(',', this)} }}.";
        }

        public static Cell Factory(int tileCount)
            => new(Enumerable.Range(0, tileCount), tileCount);

        public bool IsTotalSuperposition => Count == _maxTileCount;
        public bool IsDetermined => Count == 1;
        public bool IsFailed => Count == 0;
    }
}