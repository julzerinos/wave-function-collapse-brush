using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utility.Graph
{
    public class Graph<T> : IEnumerable<Node<T>> where T : INodeContent
    {
        public Func<int, int> GetOppositeDirection { get; }

        private readonly HashSet<Node<T>> _nodes = new();
        private readonly Dictionary<int, Node<T>> _nodeByContentLookup = new();

        private int _nodeCardinality;

        public Graph(Func<int, int> oppositeDirectionCallable)
        {
            GetOppositeDirection = oppositeDirectionCallable;
        }

        public void AddNode(Node<T> node)
        {
            _nodes.Add(node);
            _nodeByContentLookup.Add(node.Content.GetContentHash(), node);
        }

        public bool Contains(Node<T> node)
        {
            return _nodes.Contains(node);
        }

        public bool Contains(T content)
        {
            return _nodeByContentLookup.ContainsKey(content.GetContentHash());
        }

        public bool Contains(int contentHash)
        {
            return _nodeByContentLookup.ContainsKey(contentHash);
        }

        public bool GetNode(T content, out Node<T> node)
        {
            return _nodeByContentLookup.TryGetValue(content.GetContentHash(), out node);
        }

        public bool GetNode(int contentHash, out Node<T> node)
        {
            return _nodeByContentLookup.TryGetValue(contentHash, out node);
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