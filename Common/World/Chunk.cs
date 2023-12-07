using System;
using System.Threading;
using GlmSharp;
using Voxel.Common.Tile;
using Voxel.Common.Util;
using Voxel.Common.World.Storage;

namespace Voxel.Common.World;

public class Chunk : IDisposable {
    public readonly ivec3 ChunkPosition;
    public readonly ivec3 WorldPosition;

    public readonly VoxelWorld World;

    public ChunkStorage storage { get; private set; }

    private uint viewCount;

    /// <summary>
    /// When a chunk is modified, this is incremented. This is used to tell systems like networking or chunk meshing that the chunk has changed.
    /// </summary>
    private uint _version;

    public bool IsEmpty => storage is SingleStorage ss && ss.Block == Blocks.Air;

    public Chunk(ivec3 chunkPosition, VoxelWorld world, ChunkStorage? storage = null) {
        this.storage = storage ?? new SingleStorage(Blocks.Air, this);

        ChunkPosition = chunkPosition;
        WorldPosition = ChunkPosition * PositionExtensions.ChunkSize;
        World = world;
    }

    public ChunkStorage CopyStorage()
        => storage.GenerateCopy();

    public void SetStorage(ChunkStorage newStorage) {
        storage.Dispose();
        storage = newStorage;
        IncrementVersion();
    }

    public void SetBlock(ivec3 position, Block toSet) {
        storage[position] = toSet;
        IncrementVersion();
    }
    public Block GetBlock(ivec3 position)
        => storage[position];

    public uint GetVersion()
        => _version;

    public void IncrementVersion() {
        Interlocked.Increment(ref _version);
    }

    internal void IncrementViewCount() {
        viewCount++;
    }

    internal void DecrementViewCount() {
        if (viewCount != 0)
            viewCount--;

        if (viewCount == 0)
            World.UnloadChunk(ChunkPosition);
    }

    public void Dispose() {
        storage.Dispose();
    }
}
