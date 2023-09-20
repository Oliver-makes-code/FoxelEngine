using System.Collections.Generic;
using System.Numerics;

namespace Voxel.Common.World;

public class World {
    public Dictionary<ChunkPos, Chunk> chunks = new();

    public Chunk? this[ChunkPos pos] {
        get {
            chunks.TryGetValue(pos, out Chunk? chunk);
            return chunk;
        }
    }

    public void Load(ChunkPos pos) {
        chunks[pos] = new();
    }

    public void Unload(ChunkPos pos) {
        chunks.Remove(pos);
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
}
