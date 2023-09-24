using System.Collections.Generic;

namespace Voxel.Common.Tile;

public static class Blocks {
    public static readonly Dictionary<ushort, Block> BlockList = new();

    public static readonly Block Air = new(0, "air", new BlockSettings.Builder {
        IsSolidBlock = false,
    });

    public static readonly Block Stone = new(1, "stone", BlockSettings.Default);

    public static Block GetBlock(ushort id) {
        BlockList.TryGetValue(id, out var block);
        return block ?? Air;
    }
}
