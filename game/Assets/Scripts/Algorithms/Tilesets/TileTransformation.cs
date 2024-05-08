using System;

namespace Algorithms.Tilesets
{
    public struct TileTransformation
    {
        public float DegreesRotation;
        public int IndexOffset;

        public static TileTransformation[] TriangleGrid => new[]
        {
            new TileTransformation {DegreesRotation = 0, IndexOffset = 0},
            new TileTransformation {DegreesRotation = 120, IndexOffset = 1},
            new TileTransformation {DegreesRotation = 240, IndexOffset = 2},
        };
        
        public static TileTransformation[] SquareGrid => new[]
        {
            new TileTransformation {DegreesRotation = 0, IndexOffset = 0},
            new TileTransformation {DegreesRotation = 90, IndexOffset = 1},
            new TileTransformation {DegreesRotation = 180, IndexOffset = 2},
            new TileTransformation {DegreesRotation = 270, IndexOffset = 3},
        };
    }
}