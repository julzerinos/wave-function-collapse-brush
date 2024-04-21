using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Utility.Graph
{
    public class Node<T1, T2> where T2 : INodeCoordinates
    {
        [ItemCanBeNull] private readonly Node<T1, T2>[] _neighbors;

        public IEnumerable<(Node<T1, T2> neighbor, int direction)> Neighbors
            => _neighbors
                .Select((n, i) => (neighbor: n, direction: i))
                .Where(neighborDirection => neighborDirection.neighbor != null);

        public T1 Content { get; set; }
        public T2 Coordinates { get; private set; }

        public Node(int cardinality, T1 content, T2 coordinates)
        {
            _neighbors = new Node<T1, T2>[cardinality];
            Content = content;
            Coordinates = coordinates;
        }

        public void RegisterNeighbor(Node<T1, T2> neighbor, int directionIndex)
        {
            _neighbors[directionIndex] = neighbor;
        }
    }
}