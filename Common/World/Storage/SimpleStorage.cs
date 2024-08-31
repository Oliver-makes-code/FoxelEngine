using System;
using System.Collections.Generic;
using Foxel.Common.Content;
using Foxel.Common.Tile;
using Foxel.Common.Util;

namespace Foxel.Common.World.Storage;

/// <summary>
/// Stores blocks in accordance with their global ID.
///
/// TODO - Pack this somehow? Do we need to?
/// </summary>
public sealed class SimpleStorage : ChunkStorage {
    private static readonly Stack<uint[]> BlockDataCache = new();

    public readonly uint[] BlockIds;

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
    internal override Block GetBlock(int index) => ContentDatabase.Instance.Registries.Blocks.RawToEntryDirect(BlockIds[index]);
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
        var startingID = BlockIds[0];

        //If any block doesn't match starting block, cannot be reduced.
        for (var i = 1; i < BlockIds.Length; i++) {
            if (BlockIds[i] != startingID) {
                newStorage = this;
                return false;
            }
        }

        Dispose();
        newStorage = new SingleStorage(ContentDatabase.Instance.Registries.Blocks.RawToEntryDirect(startingID), target);
        return true;
    }

    public override ChunkStorage WithChunk(Chunk chunk) => GenerateCopy();
}
