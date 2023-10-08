using System;
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
        get => GetBlock((uint)index);
        set => SetBlock(value, (uint)index);
    }

    public Block this[uint index] {
        get => GetBlock(index);
        set => SetBlock(value, index);
    }

    /// <summary>
    /// Gets a block at a given local position.
    /// </summary>
    /// <param name="position"></param>
    public Block this[ivec3 position] {
        get => this[position.ToBlockIndex()];
        set => this[position.ToBlockIndex()] = value;
    }

    internal abstract void SetBlock(Block toSet, uint index);
    internal abstract Block GetBlock(uint index);

    public abstract ChunkStorage GenerateCopy();

    public abstract void Dispose();
}
