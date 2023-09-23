using System.Collections.Generic;
using System.Numerics;

namespace Voxel.Common.World;

public class World {
    public delegate void ChunkLoadEvent(ChunkPos pos);

    public event ChunkLoadEvent? OnChunkLoaded;
    public event ChunkLoadEvent? OnChunkUnloaded;

    public Dictionary<ChunkPos, Chunk> chunks = new();

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
        chunks.Remove(pos);
        OnChunkUnloaded?.Invoke(pos);
    }
    
    public ushort GetTile(BlockPos pos, bool fluid) => this[pos.ChunkPos()]?[pos.ChunkBlockPos(fluid)] ?? 0;
    public ushort GetBlock(BlockPos pos) => GetTile(pos, false);
    public ushort GetFluid(BlockPos pos) => GetTile(pos, true);
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

    public ChunkPos Up() => new(x, y+1, z);
    public ChunkPos Down() => new(x, y-1, z);
    public ChunkPos North() => new(x, y, z-1);
    public ChunkPos South() => new(x, y, z+1);
    public ChunkPos East() => new(x+1, y, z);
    public ChunkPos West() => new(x-1, y, z);
}
