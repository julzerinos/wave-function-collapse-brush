using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithms.Tilesets
{
    public enum TileTransformation
    {
        Original,
        Rotate90,
        Rotate180,
        Rotate270

        // TODO Add flips
        // TODO Add more generic rotations
    }

    public class TileData
    {
        public int OriginalIndex;
        public TileTransformation Transformation;
        public int[] TypesPerDirection;
        public HashSet<int>[] ConnectionsPerDirection;
    }

    public static class ConnectionsFromTypesParser
    {
        public static TileData[] Parse(int[][] tilesWithTypedDirections)
        {
            var tilesExtended = new List<TileData>();
            for (var index = 0; index < tilesWithTypedDirections.Length; index++)
            {
                var tileWithTypedDirections = tilesWithTypedDirections[index];
                tilesExtended.AddRange(GenerateTransformedTile(index, tileWithTypedDirections));
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
                        // TODO more holistic oppositeDirection
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

        private static IEnumerable<TileData> GenerateTransformedTile(int index, int[] connectionTypes)
        {
            foreach (TileTransformation transformation in Enum.GetValues(typeof(TileTransformation)))
            {
                var transformedTypes = new int[connectionTypes.Length];

                switch (transformation)
                {
                    case TileTransformation.Original:
                        transformedTypes = connectionTypes;
                        break;
                    case TileTransformation.Rotate270:
                        Array.Copy(connectionTypes, 1, transformedTypes, 0, connectionTypes.Length - 1);
                        transformedTypes[^1] = connectionTypes[0];
                        break;
                    case TileTransformation.Rotate180:
                        Array.Copy(connectionTypes, 2, transformedTypes, 0, connectionTypes.Length - 2);
                        Array.Copy(connectionTypes, 0, transformedTypes, 2, connectionTypes.Length - 2);
                        break;
                    case TileTransformation.Rotate90:
                        Array.Copy(connectionTypes, 0, transformedTypes, 1, connectionTypes.Length - 1);
                        transformedTypes[0] = connectionTypes[^1];
                        break;
                    default:
                        break;
                }


                var tileData = new TileData
                {
                    OriginalIndex = index,
                    Transformation = transformation,
                    TypesPerDirection = transformedTypes
                };

                yield return tileData;
            }
        }
    }
}