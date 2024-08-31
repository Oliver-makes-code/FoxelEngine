using GlmSharp;
using Foxel.Common.Tile;
using Foxel.Common.Util;

namespace Foxel.Common.World.Storage;

public abstract class ChunkStorage : IDisposable {
    /// <summary>
    /// Accesses a block at a given 
    /// </summary>
    /// <param name="index"></param>
    public Block this[int index] {
        get => GetBlock(index);
        set => SetBlock(value, index);
    }

    public Block this[uint index] {
        get => this[(int) index];
        set => this[(int)index] = value;
    }

    /// <summary>
    /// Gets a block at a given local position.
    /// </summary>
    /// <param name="position"></param>
    public Block this[ivec3 position] {
        get => this[position.ToBlockIndex()];
        set => this[position.ToBlockIndex()] = value;
    }

    public abstract ChunkStorage WithChunk(Chunk chunk);

    public abstract ChunkStorage GenerateCopy();

    public abstract void Dispose();

    internal abstract void SetBlock(Block toSet, int index);
    internal abstract Block GetBlock(int index);
}
