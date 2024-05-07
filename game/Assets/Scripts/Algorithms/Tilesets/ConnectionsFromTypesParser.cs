using System;
using System.Collections.Generic;

namespace Algorithms.Tilesets
{
    public class TileData
    {
        public int OriginalIndex;
        public TileTransformation Transformation;
        public int[] TypesPerDirection;
        public HashSet<int>[] ConnectionsPerDirection;
    }

    public static class ConnectionsFromTypesParser
    {
        public static TileData[] Parse(int[][] tilesWithTypedDirections, TileTransformation[] transformations)
        {
            var tilesExtended = new List<TileData>();
            for (var index = 0; index < tilesWithTypedDirections.Length; index++)
            {
                var tileWithTypedDirections = tilesWithTypedDirections[index];
                tilesExtended.AddRange(GenerateTransformedTile(index, tileWithTypedDirections, transformations));
            }

            foreach (var tileData in tilesExtended)
            {
                var connectionsPerDirection = new List<HashSet<int>>();
                var directions = tileData.TypesPerDirection;

                for (var direction = 0; direction < directions.Length; direction++)
                {
                    var connectionsSet = new HashSet<int>();
                    var type = directions[direction];

                    for (var otherTile = 0; otherTile < tilesExtended.Count; otherTile++)
                    {
                        // TODO oppositeDirection assumes uniform tiling
                        var oppositeDirection = (direction + directions.Length / 2) % directions.Length;
                        var otherType = tilesExtended[otherTile].TypesPerDirection[oppositeDirection];

                        if (type == otherType)
                            connectionsSet.Add(otherTile);
                    }

                    connectionsPerDirection.Add(connectionsSet);
                }

                tileData.ConnectionsPerDirection = connectionsPerDirection.ToArray();
            }

            return tilesExtended.ToArray();
        }

        private static IEnumerable<TileData> GenerateTransformedTile(int index, int[] connectionTypes, TileTransformation[] transformations)
        {
            foreach (var transformation in transformations)
            {
                var transformedTypes = new int[connectionTypes.Length];

                if (transformation.DegreesRotation == 0f)
                {
                    yield return new TileData
                    {
                        OriginalIndex = index,
                        Transformation = transformation,
                        TypesPerDirection = connectionTypes
                    };
                    continue;
                }

                Array.Copy(
                    connectionTypes,
                    0,
                    transformedTypes,
                    transformation.IndexOffset,
                    connectionTypes.Length - transformation.IndexOffset
                );
                Array.Copy(
                    connectionTypes,
                    connectionTypes.Length - transformation.IndexOffset,
                    transformedTypes,
                    0,
                    transformation.IndexOffset
                );

                yield return new TileData
                {
                    OriginalIndex = index,
                    Transformation = transformation,
                    TypesPerDirection = transformedTypes
                };
            }
        }
    }
}