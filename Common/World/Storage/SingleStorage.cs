using Foxel.Common.Util;
using Foxel.Common.World.Content;
using Foxel.Common.World.Content.Blocks;
using Foxel.Common.World.Content.Blocks.State;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Storage;

/// <summary>
/// Storage that just stores a single block.
///
/// When a new block is assigned to this storage, it automatically swaps itself out for a SimpleStorage with the appropriate data.
/// </summary>
public sealed class SingleStorage : ChunkStorage {
    public static new readonly Codec<ChunkStorage> Codec = new ProxyCodec<int, ChunkStorage>(
        Codecs.Int,
        (block) => new SingleStorage(ContentStores.Blocks.GetValue(block).DefaultState, null),
        (storage) => ContentStores.Blocks.GetId(((SingleStorage)storage).Block.Block)
    );

    public readonly Chunk? Chunk;
    public readonly BlockState Block;

    public SingleStorage(BlockState block, Chunk? chunk) {
        Block = block;
        Chunk = chunk;
    }

    public override void Dispose() {}

    public override ChunkStorage GenerateCopy()
        => new SingleStorage(Block, Chunk);
        
    public override Codec<ChunkStorage> GetCodec()
        => Codec;

    public override ChunkStorage WithChunk(Chunk chunk)
        => new SingleStorage(Block, chunk);

    internal override BlockState GetBlock(int index)
        => Block;

    internal override void SetBlock(BlockState toSet, int index) {
        if (toSet == Block)
            return;

        //Upgrade chunk storage if the new block is different.
        var newStorage = new SimpleStorage(Block);
        newStorage.SetBlock(toSet, index);

        Chunk?.SetStorage(newStorage);
    }
}
