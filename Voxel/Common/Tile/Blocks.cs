using System.Collections.Generic;

namespace Voxel.Common.Tile;

public static class Blocks {
    private static List<Block> _blockList;

    private static readonly Block[] BlockArray;
    private static readonly Dictionary<string, Block> BlocksByName = new();

    public static readonly Block Air;
    public static readonly Block Stone;
    static Blocks() {
        _blockList = new();

        Air = RegisterBlock(new Block("air", new BlockSettings.Builder {
            IsSolidBlock = false
        }));

        Stone = RegisterBlock(new Block("stone"));

        BlockArray = _blockList.ToArray();
        _blockList = null;
    }

    private static T RegisterBlock<T>(T toRegister) where T : Block {
        var id = (uint)_blockList.Count;
        toRegister.Id = id;
        _blockList.Add(toRegister);
        BlocksByName[toRegister.Name] = toRegister;
        return toRegister;
    }

    public static Block GetBlock(uint id) => BlockArray[id];
    public static Block? GetBlock(string name) {
        BlocksByName.TryGetValue(name, out var ret);
        return ret;
    }
}
