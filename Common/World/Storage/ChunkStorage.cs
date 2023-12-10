using GlmSharp;
using Voxel.Common.Tile;
using Voxel.Common.Util;

namespace Voxel.Common.World.Storage;

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

    internal abstract void SetBlock(Block toSet, int index);
    internal abstract Block GetBlock(int index);

    public abstract ChunkStorage GenerateCopy();

    public abstract void Dispose();
}
