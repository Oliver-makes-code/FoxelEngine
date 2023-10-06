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

    private ChunkStorage storage;

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

    public void SetStorage(ChunkStorage storage) {
        this.storage = storage;
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
    
    public void IncrementVersion()
        => Interlocked.Increment(ref _version);


    public void Dispose() {
        storage.Dispose();
    }
}
