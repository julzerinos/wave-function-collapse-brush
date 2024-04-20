using Algorithms.Tilesets;

namespace Algorithms.WaveFunctionCollapse.Input
{
    public interface IWaveFunctionInput
    {
        public int TileCount { get; }
        public string[] Tiles { get; }
        public TileData[] TileData { get; }
    }
}