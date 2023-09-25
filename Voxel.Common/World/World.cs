using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Voxel.Common.Tile;

namespace Voxel.Common.World;

public class World {
    public delegate void ChunkLoadEvent(ChunkPos[] chunks);

    public event ChunkLoadEvent? OnChunkLoaded;
    public event ChunkLoadEvent? OnChunkUnloaded;

    public ConcurrentDictionary<ChunkPos, Chunk> chunks = new();

    public ConcurrentQueue<ChunkPos> ChunksToLoad = new();
    public ConcurrentQueue<ChunkPos> ChunksToRemove = new();

    private Thread _chunkLoadingThread = new(o => {
        var self = o as World ?? throw new InvalidOperationException();
        while (true) {
            List<ChunkPos> unloaded = new();
            while (self.ChunksToRemove.TryDequeue(out var toRemove)) {
                if (!self.IsChunkLoaded(toRemove))
                    continue;
                self.Unload(toRemove);
                unloaded.Add(toRemove);
            }
            if (unloaded.Count != 0)
                self.OnChunkUnloaded?.Invoke(unloaded.ToArray());

            List<ChunkPos> loaded = new();
            while (self.ChunksToLoad.TryDequeue(out var toAdd)) {
                if (self.IsChunkLoaded(toAdd)) 
                    continue;
                self.Load(toAdd, out var chunk);
                chunk.FillWithSimplexNoise(toAdd);
                loaded.Add(toAdd);
            }
            if (loaded.Count != 0)
                self.OnChunkLoaded?.Invoke(loaded.ToArray());
        }
    });

    public Chunk? this[ChunkPos pos] {
        get {
            chunks.TryGetValue(pos, out Chunk? chunk);
            return chunk;
        }
    }

    public bool IsChunkLoaded(ChunkPos pos) => chunks.ContainsKey(pos);

    public void Load(ChunkPos pos, out Chunk chunk) {
        if (IsChunkLoaded(pos)) {
            chunk = chunks[pos];
            return;
        }
        chunk = new();
        chunks[pos] = chunk;
    }

    public void Unload(ChunkPos pos) {
        if (!IsChunkLoaded(pos))
            return;
        chunks.Remove(pos, out _);
    }
    
    public ushort GetTile(BlockPos pos, bool fluid) => this[pos.ChunkPos()]?[pos.ChunkBlockPos(fluid)] ?? 0;
    public Block GetBlock(BlockPos pos) => Blocks.GetBlock(GetTile(pos, false));
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
        var hashCode = x;
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
    public Chunk[] chunks;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIdx(int x, int y, int z) => x * 9 + y * 3 + z;
    
    public ChunkView(World world, ChunkPos pos) {
        this.pos = pos + new ChunkPos(-1, -1, -1);
        chunks = new Chunk[27];

        for (var x = 0; x < 3; x++) {
            for (var y = 0; y < 3; y++) {
                for (var z = 0; z < 3; z++) {
                    chunks[GetIdx(x,y,z)] = world[pos + new ChunkPos(x-1,y-1,z-1)] ?? Chunk.Full;
                }
            }
        }
    }

    public ushort GetTile(BlockPos blockPos, bool fluid)
        => chunks[GetIdx(
            (blockPos.x >> 5) - pos.x,
            (blockPos.y >> 5) - pos.y,
            (blockPos.z >> 5) - pos.z
        )][
            ChunkBlockPos.GetRawFrom(fluid, blockPos.x, blockPos.y, blockPos.z)
        ];
    
    public Block GetBlock(BlockPos blockPos) => Blocks.GetBlock(GetTile(blockPos, false));
    
    public ushort GetFluid(BlockPos blockPos) => GetTile(blockPos, true);
}
