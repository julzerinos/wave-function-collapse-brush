using System.Collections.Generic;
using System.Linq;
using Utility.Graph;

namespace Algorithms.WaveFunctionCollapse
{
    public class Cell : HashSet<int>, INodeContent
    {
        public (float x, float y) Coordinates { get; }

        public Cell(IEnumerable<int> collection, (float x, float y) coordinates) : base(collection)
        {
            Coordinates = coordinates;
        }

        public int GetContentHash()
        {
            return Coordinates.GetHashCode();
        }

        public override string ToString()
        {
            return $"Cell of tiles {{ {string.Join(',', this)} }} at {Coordinates}.";
        }

        public static Cell Factory((float x, float y) position, int tileCount)
            => new(Enumerable.Range(0, tileCount), position);
    }
}