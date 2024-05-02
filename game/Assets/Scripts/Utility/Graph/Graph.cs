using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Graph
{
    public class Graph<T> : IEnumerable<Node<T>>
    {
        public Func<int, int> GetOppositeDirection { get; }
        public int NodeCardinality { get; }

        public Func<T> ContentFactory { get; }
        public IEnumerable<INodeCoordinates> NeighborOffsetsGenerator { get; }

        private readonly HashSet<Node<T>> _nodes = new();
        private readonly Dictionary<INodeCoordinates, Node<T>> _nodeByCoordinatesLookup = new();


        public Graph(
            Func<int, int> oppositeDirectionCallable,
            int nodeCardinality,
            Func<T> contentFactory,
            IEnumerable<INodeCoordinates> neighborOffsetsGenerator
        )
        {
            GetOppositeDirection = oppositeDirectionCallable;
            NodeCardinality = nodeCardinality;
            ContentFactory = contentFactory;
            NeighborOffsetsGenerator = neighborOffsetsGenerator;
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

        public bool Contains(Node<T> node)
        {
            return _nodes.Contains(node);
        }
        
        public bool Contains(INodeCoordinates coordinates)
        {
            return _nodeByCoordinatesLookup.ContainsKey(coordinates);
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