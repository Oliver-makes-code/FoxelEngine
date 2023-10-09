using Voxel.Common.Tile;

namespace Voxel.Common.World.Storage;

/// <summary>
/// Storage that just stores a single block.
///
/// When a new block is assigned to this storage, it automatically swaps itself out for a SimpleStorage with the appropriate data.
/// </summary>
public class SingleStorage : ChunkStorage {
    public readonly Chunk Chunk;
    public readonly Block Block;

    public SingleStorage(Block block, Chunk chunk) {
        Block = block;
        Chunk = chunk;
    }

    internal override Block GetBlock(uint index) => Block;
    public override void Dispose() {}

    internal override void SetBlock(Block toSet, uint index) {
        if (toSet == Block)
            return;

        //Upgrade chunk storage if the new block is different.
        var newStorage = new SimpleStorage(Block);
        newStorage.SetBlock(toSet, index);

        Chunk.SetStorage(newStorage);
    }

    public override ChunkStorage GenerateCopy() => new SingleStorage(Block, Chunk);
}
