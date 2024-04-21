using System.Collections.Generic;
using System.Linq;
using Utility.Graph;

namespace Algorithms.WaveFunctionCollapse.WaveGraph
{
    public class Cell : HashSet<int>, INodeCoordinates
    {
        public Cell(IEnumerable<int> collection) : base(collection) { }

        public override string ToString()
        {
            return $"Cell of tiles {{ {string.Join(',', this)} }}.";
        }

        public static Cell Factory(int tileCount)
            => new(Enumerable.Range(0, tileCount));
    }
}