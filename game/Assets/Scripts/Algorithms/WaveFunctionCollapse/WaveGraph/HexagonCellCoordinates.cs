using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Graph;

namespace Algorithms.WaveFunctionCollapse.WaveGraph
{
    [Serializable]
    public struct HexagonCellCoordinates : INodeCoordinates
    {
        public int X;
        public int Y;


        public HexagonCellCoordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

        public HexagonCellCoordinates(Vector2Int vec2i)
        {
            X = Mathf.RoundToInt(vec2i.x / 2f);
            Y = vec2i.y;
        }

        public static HexagonCellCoordinates operator +(HexagonCellCoordinates a, HexagonCellCoordinates b) =>
            new() { X = a.X + b.X, Y = a.Y + b.Y, };

        public override bool Equals(object obj) =>
            obj is HexagonCellCoordinates otherCoordinates
            && X.Equals(otherCoordinates.X)
            && Y.Equals(otherCoordinates.Y);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(X);
            hash.Add(Y);
            return hash.ToHashCode();
        }

        public override string ToString() => $"hex({X}, {Y})";

        private const float Horizontal = 3f / 2;
        private static float Vertical = Mathf.Sqrt(3) / 2;

        public Vector3 ToPhysicalSpace
        {
            get
            {
                var x = X / 2;
                var z = Y;
                return new Vector3(x, 0, z);
            }
        }

        public INodeCoordinates Add(INodeCoordinates other)
        {
            if (other is not HexagonCellCoordinates hexagonCellCoordinates)
                throw new Exception("[HexagonCellCoordinates] tried adding non-square coordinates.");

            return this + hexagonCellCoordinates;
        }
    }
}