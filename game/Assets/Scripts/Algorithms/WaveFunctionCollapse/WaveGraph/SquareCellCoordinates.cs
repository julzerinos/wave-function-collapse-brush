using System;
using UnityEngine;
using Utility.Graph;

namespace Algorithms.WaveFunctionCollapse.WaveGraph
{
    [Serializable]
    public struct SquareCellCoordinates : INodeCoordinates
    {
        public int X;
        public int Y;

        public SquareCellCoordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

        public SquareCellCoordinates(Vector2Int vec2i)
        {
            X = vec2i.x;
            Y = vec2i.y;
        }

        public static SquareCellCoordinates operator +(SquareCellCoordinates a, SquareCellCoordinates b) =>
            new() { X = b.X + a.X, Y = b.Y + a.Y, };

        public override bool Equals(object obj) =>
            obj is SquareCellCoordinates otherCoordinates && X.Equals(otherCoordinates.X) && Y.Equals(otherCoordinates.Y);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(X);
            hash.Add(Y);
            return hash.ToHashCode();
        }

        public override string ToString() => $"cc({X}, {Y})";


        public Vector3 ToPhysicalSpace => new Vector3(X, 0, Y);

        public INodeCoordinates Add(INodeCoordinates other)
        {
            if (other is not SquareCellCoordinates squareCellCoordinates)
                throw new Exception("[SquareCellCoordinates] tried adding non-square coordinates.");

            return this + squareCellCoordinates;
        }
    }
}