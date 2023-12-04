using System;
using System.Collections.Generic;
using Voxel.Common.Tile;
using Voxel.Common.Util;

namespace Voxel.Common.World.Storage;

/// <summary>
/// Stores blocks in accordance with their global ID.
///
/// TODO - Pack this somehow? Do we need to?
/// </summary>
public class SimpleStorage : ChunkStorage {
    private static readonly Stack<uint[]> BlockDataCache = new();

    private readonly uint[] BlockIds;

    public SimpleStorage() {
        BlockIds = GetBlockData();
    }

    public SimpleStorage(Block fill) : this() {
        Array.Fill(BlockIds, fill.id);
    }

    private static uint[] GetBlockData() {
        lock (BlockDataCache)
            if (BlockDataCache.TryPop(out var value))
                return value;

        return new uint[PositionExtensions.ChunkCapacity];
    }

    internal override void SetBlock(Block toSet, int index) => BlockIds[index] = toSet.id;
    internal override Block GetBlock(int index) => Blocks.GetBlock(BlockIds[index]);
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
}
