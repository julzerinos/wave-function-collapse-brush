using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Graph;

namespace Algorithms.WaveFunctionCollapse.WaveGraph
{
    [Serializable]
    public struct HexagonCellCoordinates : INodeCoordinates
    {
        public int Q;
        public int R;
        public int S;

        public HexagonCellCoordinates(int q, int r, int s)
        {
            Q = q;
            R = r;
            S = s;
        }

        public HexagonCellCoordinates(Vector2Int vec2i)
        {
            var q = vec2i.x;
            var r = (vec2i.y - vec2i.x) / 2;

            Q = Mathf.RoundToInt(q);
            R = Mathf.RoundToInt(r);
            S = Mathf.RoundToInt(-Q - R);
        }

        public static HexagonCellCoordinates operator +(HexagonCellCoordinates a, HexagonCellCoordinates b) =>
            new() { Q = b.Q + a.Q, R = b.R + a.R, };

        public override bool Equals(object obj) =>
            obj is HexagonCellCoordinates otherCoordinates
            && Q.Equals(otherCoordinates.Q)
            && R.Equals(otherCoordinates.R)
            && S.Equals(otherCoordinates.S);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Q);
            hash.Add(R);
            hash.Add(S);
            return hash.ToHashCode();
        }

        public override string ToString() => $"hex({Q}, {R}, {S})";

        private const float Horizontal = 3f / 2;
        private static float Vertical = Mathf.Sqrt(3) / 2;

        public Vector3 ToPhysicalSpace
        {
            get
            {
                var x = Q;
                var z = 2 * (R + Q);
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