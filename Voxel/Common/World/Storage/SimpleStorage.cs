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

    private readonly uint[] _blockIds;

    public SimpleStorage() {
        _blockIds = GetBlockData();
    }

    public SimpleStorage(Block fill) : this() {
        Array.Fill(_blockIds, fill.Id);
    }

    private static uint[] GetBlockData() {
        lock (BlockDataCache)
            if (BlockDataCache.TryPop(out var value))
                return value;

        return new uint[PositionExtensions.CHUNK_CAPACITY];
    }

    internal override void SetBlock(Block toSet, uint index) => _blockIds[index] = toSet.Id;
    internal override Block GetBlock(uint index) => Blocks.GetBlock(_blockIds[index]);
    public override ChunkStorage GenerateCopy() {
        var newStorage = new SimpleStorage();
        _blockIds.CopyTo(newStorage._blockIds.AsSpan());
        return newStorage;
    }

    public override void Dispose() {
        lock (BlockDataCache)
            BlockDataCache.Push(_blockIds);
    }
}
