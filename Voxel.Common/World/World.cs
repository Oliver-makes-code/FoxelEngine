using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace Voxel.Common.World;

public class World {
    public delegate void ChunkLoadEvent(ChunkPos pos);

    public event ChunkLoadEvent? OnChunkLoaded;
    public event ChunkLoadEvent? OnChunkUnloaded;

    public ConcurrentDictionary<ChunkPos, Chunk> chunks = new();

    public ConcurrentQueue<ChunkPos> ChunksToLoad = new();
    public ConcurrentQueue<ChunkPos> ChunksToRemove = new();

    private Thread _chunkLoadingThread = new(o => {
        var self = o as World ?? throw new InvalidOperationException();
        while (true) {
            if (self.ChunksToRemove.TryDequeue(out var toRemove) && self.chunks.ContainsKey(toRemove)) {
                self.Unload(toRemove);
            }

            if (self.ChunksToLoad.TryDequeue(out var toAdd)) {
                if (self.chunks.ContainsKey(toAdd)) {
                    LogUtil.PlatformLogger.Debug($"Skipping already loaded chunk {toAdd}");
                } else {
                    self.Load(toAdd);
                    self[toAdd]!.FillWithSimplexNoise(toAdd);
                }
            }
        }
    });

    public Chunk? this[ChunkPos pos] {
        get {
            chunks.TryGetValue(pos, out Chunk? chunk);
            return chunk;
        }
    }

    public bool IsChunkLoaded(ChunkPos pos) => chunks.ContainsKey(pos);

    public void Load(ChunkPos pos) {
        if (IsChunkLoaded(pos))
            return;
        chunks[pos] = new();
        OnChunkLoaded?.Invoke(pos);
    }

    public void Unload(ChunkPos pos) {
        if (!IsChunkLoaded(pos))
            return;
        chunks.Remove(pos, out _);
        OnChunkUnloaded?.Invoke(pos);
    }
    
    public ushort GetTile(BlockPos pos, bool fluid) => this[pos.ChunkPos()]?[pos.ChunkBlockPos(fluid)] ?? 0;
    public ushort GetBlock(BlockPos pos) => GetTile(pos, false);
    public ushort GetFluid(BlockPos pos) => GetTile(pos, true);

    public World() {
        _chunkLoadingThread.Start(this);
    }

    public void OnExiting() {
        _chunkLoadingThread.Interrupt();
    }
}

public readonly struct ChunkPos {
    public readonly int x;
    public readonly int y;
    public readonly int z;

    public ChunkPos(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 ToVector() => new(x * 32, y * 32, z * 32);

    public override int GetHashCode() {
        int hashCode = x;
        hashCode *= 23;
        hashCode += y;
        hashCode *= 23;
        hashCode += z;
        return hashCode;
    }

    public static ChunkPos operator + (ChunkPos a, ChunkPos b)
        => new(a.x+b.x, a.y+b.y, a.z+b.z);

    public static ChunkPos operator - (ChunkPos a, ChunkPos b)
        => new(a.x-b.x, a.y-b.y, a.z-b.z);

    public ChunkPos Up() => new(x, y+1, z);
    public ChunkPos Down() => new(x, y-1, z);
    public ChunkPos North() => new(x, y, z-1);
    public ChunkPos South() => new(x, y, z+1);
    public ChunkPos East() => new(x+1, y, z);
    public ChunkPos West() => new(x-1, y, z);

    public override string ToString() => $"({x}, {y}, {z})";
}

public class ChunkView {
    public ChunkPos pos;
    public Chunk[,,] chunks;

    public ChunkView(World world, ChunkPos pos) {
        this.pos = pos + new ChunkPos(-1, -1, -1);
        chunks = new Chunk[3,3,3];

        for (var x = 0; x < 3; x++) {
            for (var y = 0; y < 3; y++) {
                for (var z = 0; z < 3; z++) {
                    chunks[x,y,z] = world[pos + new ChunkPos(x-1,y-1,z-1)] ?? Chunk.Empty;
                }
            }
        }
    }

    public ushort GetTile(BlockPos blockPos, bool fluid)
        => chunks[
            (blockPos.x >> 5) - pos.x,
            (blockPos.y >> 5) - pos.y,
            (blockPos.z >> 5) - pos.z
        ][
            ChunkBlockPos.GetRawFrom(fluid, (byte)blockPos.x, (byte)blockPos.y, (byte)blockPos.z)
        ];

    public ushort GetBlock(BlockPos blockPos) => GetTile(blockPos, false);

    public ushort GetFluid(BlockPos blockPos) => GetTile(blockPos, true);
}
