using Foxel.Common.Util;
using Foxel.Common.World.Content;
using Foxel.Common.World.Content.Blocks;
using Foxel.Common.World.Content.Blocks.State;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Storage;

/// <summary>
/// Stores blocks in accordance with their global ID.
///
/// TODO - Pack this somehow? Do we need to?
/// </summary>
public sealed class SimpleStorage : ChunkStorage {
    public static new readonly Codec<ChunkStorage> Codec = new ProxyCodec<int[], ChunkStorage>(
        Codecs.Int.FixedArray(PositionExtensions.ChunkCapacity),
        (arr) => new SimpleStorage(arr),
        (storage) => ((SimpleStorage)storage).BlockIds
    );

    private static readonly Stack<int[]> BlockDataCache = new();

    public readonly int[] BlockIds;

    public SimpleStorage() {
        BlockIds = GetBlockData();
    }

    public SimpleStorage(int[] blockIds) {
        BlockIds = blockIds;
    }

    public SimpleStorage(BlockState fill) : this() {
        Array.Fill(BlockIds, ContentStores.Blocks.GetId(fill.Block));
    }

    private static int[] GetBlockData() {
        lock (BlockDataCache)
            if (BlockDataCache.TryPop(out var value))
                return value;

        return new int[PositionExtensions.ChunkCapacity];
    }
    public override ChunkStorage GenerateCopy() {
        var newStorage = new SimpleStorage();
        BlockIds.CopyTo(newStorage.BlockIds.AsSpan());
        return newStorage;
    }

    public override void Dispose() {
        lock (BlockDataCache) {
            BlockDataCache.Push(BlockIds);
            //Console.Out.WriteLine($"{BlockDataCache.Count} block caches on stack");
        }
    }

    public bool ReduceIfPossible(Chunk target, out ChunkStorage newStorage) {
        int startingId = BlockIds[0];

        //If any block doesn't match starting block, cannot be reduced.
        for (var i = 1; i < BlockIds.Length; i++) {
            if (BlockIds[i] != startingId) {
                newStorage = this;
                return false;
            }
        }

        Dispose();
        newStorage = new SingleStorage(ContentStores.Blocks.GetValue(startingId).DefaultState, target);
        return true;
    }

    public override ChunkStorage WithChunk(Chunk chunk)
        => GenerateCopy();
        
    public override Codec<ChunkStorage> GetCodec()
        => Codec;

    internal override void SetBlock(BlockState toSet, int index)
        => BlockIds[index] = ContentStores.Blocks.GetId(toSet.Block);

    internal override BlockState GetBlock(int index)
        => ContentStores.Blocks.GetValue(BlockIds[index]).DefaultState;
}
