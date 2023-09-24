using System.Collections.Generic;

namespace Voxel.Common.Tile;

public static class Blocks {
    public static readonly Block[] BlockList = new Block[2048];

    public static readonly Block Air = new(0, "air", new BlockSettings.Builder {
        IsSolidBlock = false,
    });

    public static readonly Block Stone = new(1, "stone", BlockSettings.Default);

    public static Block GetBlock(ushort id) => BlockList[id >> 5] ?? Air;
}
