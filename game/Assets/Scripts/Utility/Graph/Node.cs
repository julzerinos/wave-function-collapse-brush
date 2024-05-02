using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Utility.Graph
{
    public class Node<T1>
    {
        [ItemCanBeNull] private readonly Node<T1>[] _neighbors;

        public IEnumerable<(Node<T1> neighbor, int direction)> Neighbors
            => _neighbors
                .Select((n, i) => (neighbor: n, direction: i))
                .Where(neighborDirection => neighborDirection.neighbor != null);

        public T1 Content { get; set; }
        public INodeCoordinates Coordinates { get; private set; }

        public Node(int cardinality, T1 content, INodeCoordinates coordinates)
        {
            _neighbors = new Node<T1>[cardinality];
            Content = content;
            Coordinates = coordinates;
        }

        public void RegisterNeighbor(Node<T1> neighbor, int directionIndex)
        {
            _neighbors[directionIndex] = neighbor;
        }

        public bool GetNeighborAtDirection(int direction, out Node<T1> node)
        {
            node = _neighbors[direction];
            return node is not null;
        }
    }
}