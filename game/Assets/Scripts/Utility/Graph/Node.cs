using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Utility.Graph
{
    public class Node<T> where T : INodeContent
    {
        [ItemCanBeNull] private readonly Node<T>[] _neighbors;

        public IEnumerable<(Node<T> neighbor, int direction)> Neighbors
            => _neighbors
                .Select((n, i) => (neighbor: n, direction: i))
                .Where(neighborDirection => neighborDirection.neighbor != null);

        public T Content { get; set; }

        public Node(int cardinality, T content)
        {
            _neighbors = new Node<T>[cardinality];
            Content = content;
        }

        public void RegisterNeighbor(Node<T> neighbor, int directionIndex)
        {
            _neighbors[directionIndex] = neighbor;
        }
    }
}