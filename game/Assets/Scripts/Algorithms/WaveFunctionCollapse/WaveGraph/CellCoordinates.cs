using System;
using UnityEngine;
using Utility.Graph;

namespace Algorithms.WaveFunctionCollapse.WaveGraph
{
    public struct CellCoordinates : INodeCoordinates
    {
        public float X;
        public float Y;

        public CellCoordinates(float x, float y)
        {
            X = x;
            Y = y;
        }
        public CellCoordinates(Vector2Int vec2i)
        {
            X = vec2i.x;
            Y = vec2i.y;
        }
        public static CellCoordinates operator +(CellCoordinates a, CellCoordinates b) =>
            new() { X = b.X + a.X, Y = b.Y + a.Y };

        public override bool Equals(object obj) =>
            obj is CellCoordinates otherCoordinates && X.Equals(otherCoordinates.X) && Y.Equals(otherCoordinates.Y);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(X);
            hash.Add(Y);
            return hash.ToHashCode();
        }

        public override string ToString() => $"cc({X}, {Y})";
        
        
    }
}