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
        public int NodeCardinality { get; }

        public Func<T1> ContentFactory { get; }
        public IEnumerable<T2> NeighborOffsetsGenerator { get; }

        private readonly HashSet<Node<T1, T2>> _nodes = new();
        private readonly Dictionary<INodeCoordinates, Node<T1, T2>> _nodeByCoordinatesLookup = new();


        public Graph(
            Func<int, int> oppositeDirectionCallable,
            int nodeCardinality,
            Func<T1> contentFactory,
            IEnumerable<T2> neighborOffsetsGenerator
        )
        {
            GetOppositeDirection = oppositeDirectionCallable;
            NodeCardinality = nodeCardinality;
            ContentFactory = contentFactory;
            NeighborOffsetsGenerator = neighborOffsetsGenerator;
        }

        public void AddNode(Node<T1, T2> node)
        {
            _nodes.Add(node);
            _nodeByCoordinatesLookup.Add(node.Coordinates, node);
        }

        public bool GetNode(T2 coordinates, out Node<T1, T2> node)
        {
            return _nodeByCoordinatesLookup.TryGetValue(coordinates, out node);
        }

        public bool Contains(Node<T1, T2> node)
        {
            return _nodes.Contains(node);
        }
        
        public bool Contains(T2 coordinates)
        {
            return _nodeByCoordinatesLookup.ContainsKey(coordinates);
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