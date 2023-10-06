using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Voxel.Common.Tile;

public static class Blocks {
    private static readonly Block[] BlockArray;
    private static readonly Dictionary<string, Block> BlocksByName = new();

    public static readonly Block Air;
    public static readonly Block Stone;
    
    private static List<Block>? blockList;

    static Blocks() {
        blockList = new();

        Air = RegisterBlock(new Block("air", new BlockSettings.Builder {
            IsSolidBlock = false
        }));

        Stone = RegisterBlock(new Block("stone"));

        BlockArray = blockList.ToArray();
        blockList = null;
    }

    private static T RegisterBlock<T>(T toRegister) where T : Block {
        uint id = (uint)blockList!.Count;
        toRegister.id = id;
        blockList.Add(toRegister);
        BlocksByName[toRegister.Name] = toRegister;
        return toRegister;
    }

    public static Block GetBlock(uint id)
        => BlockArray[id];
    public static bool GetBlock(string name, [NotNullWhen(true)] out Block? block)
        => BlocksByName.TryGetValue(name, out block);
    
    public static Block? GetBlock(string name) {
        GetBlock(name, out var block);
        return block;
    }
}
