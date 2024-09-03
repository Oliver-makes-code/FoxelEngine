using Foxel.Common.Content;
using Foxel.Common.Tile;
using Foxel.Common.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Storage;

/// <summary>
/// Storage that just stores a single block.
///
/// When a new block is assigned to this storage, it automatically swaps itself out for a SimpleStorage with the appropriate data.
/// </summary>
public sealed class SingleStorage : ChunkStorage {
    public static readonly Codec<ChunkStorage> Codec = new ProxyCodec<uint, ChunkStorage>(
        Codecs.UInt,
        (block) => new SingleStorage(ContentDatabase.Instance.Registries.Blocks.RawToEntryDirect(block), null),
        (storage) => ((SingleStorage)storage).Block.id
    );

    public readonly Chunk? Chunk;
    public readonly Block Block;

    public SingleStorage(Block block, Chunk? chunk) {
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

    internal override Block GetBlock(int index)
        => Block;

    internal override void SetBlock(Block toSet, int index) {
        if (toSet == Block)
            return;

        //Upgrade chunk storage if the new block is different.
        var newStorage = new SimpleStorage(Block);
        newStorage.SetBlock(toSet, index);

        Chunk?.SetStorage(newStorage);
    }
}
