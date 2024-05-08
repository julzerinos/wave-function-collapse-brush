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

        private readonly HashSet<Node<T>> _nodes = new();

        private readonly Dictionary<T, Node<T>> _nodeByContentLookup = new();

        public Graph(
            Func<int, int> oppositeDirectionCallable,
            int nodeCardinality
        )
        {
            GetOppositeDirection = oppositeDirectionCallable;
            NodeCardinality = nodeCardinality;
        }

        public void AddNode(Node<T> node)
        {
            _nodes.Add(node);
            _nodeByContentLookup.Add(node.Content, node);
        }

        public bool Contains(Node<T> node)
        {
            return _nodes.Contains(node);
        }

        public bool Contains(T content)
        {
            return _nodeByContentLookup.ContainsKey(content);
        }

        public bool GetNode(T content, out Node<T> node)
        {
            return _nodeByContentLookup.TryGetValue(content, out node);
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