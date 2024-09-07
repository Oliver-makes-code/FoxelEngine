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
    public static new readonly Codec<ChunkStorage> Codec = new ProxyCodec<BlockState, ChunkStorage>(
        BlockState.Codec,
        (state) => new SingleStorage(state, null),
        (storage) => ((SingleStorage)storage).State
    );

    public readonly Chunk? Chunk;
    public readonly BlockState State;

    public SingleStorage(BlockState state, Chunk? chunk) {
        State = state;
        Chunk = chunk;
    }

    public override void Dispose() {}

    public override ChunkStorage GenerateCopy()
        => new SingleStorage(State, Chunk);
        
    public override Codec<ChunkStorage> GetCodec()
        => Codec;

    public override ChunkStorage WithChunk(Chunk chunk)
        => new SingleStorage(State, chunk);

    internal override BlockState GetBlock(int index)
        => State;

    internal override void SetBlock(BlockState toSet, int index) {
        if (toSet == State)
            return;

        //Upgrade chunk storage if the new block is different.
        var newStorage = new SimpleStorage(State);
        newStorage.SetBlock(toSet, index);

        Chunk?.SetStorage(newStorage);
    }
}
