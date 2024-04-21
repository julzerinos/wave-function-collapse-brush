using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utility.Graph
{
    public class Graph<T> : IEnumerable<Node<T>> where T : INodeCoordinates
    {
        public Func<int, int> GetOppositeDirection { get; }

        private readonly HashSet<Node<T>> _nodes = new();
        private readonly Dictionary<INodeCoordinates, Node<T>> _nodeByCoordinatesLookup = new();

        private int _nodeCardinality;

        public Graph(Func<int, int> oppositeDirectionCallable)
        {
            GetOppositeDirection = oppositeDirectionCallable;
        }

        public void AddNode(Node<T> node)
        {
            _nodes.Add(node);
            _nodeByCoordinatesLookup.Add(node.Coordinates, node);
        }

        public bool GetNode(INodeCoordinates coordinates, out Node<T> node)
        {
            return _nodeByCoordinatesLookup.TryGetValue(coordinates, out node);
        }

        public Node<T> GetRandomNode()
        {
            return _nodes.ElementAt(0);
        }

        public IEnumerator<Node<T>> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}