using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Graph
{
    public class Graph<T1, T2> : IEnumerable<Node<T1, T2>>
        where T2 : INodeCoordinates
    {
        public Func<int, int> GetOppositeDirection { get; }

        private readonly HashSet<Node<T1, T2>> _nodes = new();
        private readonly Dictionary<INodeCoordinates, Node<T1, T2>> _nodeByCoordinatesLookup = new();

        private int _nodeCardinality;

        public Graph(Func<int, int> oppositeDirectionCallable)
        {
            GetOppositeDirection = oppositeDirectionCallable;
        }

        public void AddNode(Node<T1, T2> node)
        {
            _nodes.Add(node);
            _nodeByCoordinatesLookup.Add(node.Coordinates, node);
        }

        public bool GetNode(INodeCoordinates coordinates, out Node<T1, T2> node)
        {
            return _nodeByCoordinatesLookup.TryGetValue(coordinates, out node);
        }

        public Node<T1, T2> GetRandomNode()
        {
            return _nodes.ElementAt(0);
        }

        public IEnumerator<Node<T1, T2>> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}