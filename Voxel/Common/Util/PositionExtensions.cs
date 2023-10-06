using System.Runtime.CompilerServices;
using GlmSharp;

namespace Voxel.Common.Util;

public static class PositionExtensions {

    public const int ChunkSize = 32;
    public const int ChunkStep = ChunkSize * ChunkSize;
    public const int ChunkCapacity = ChunkStep * ChunkSize;

    public static int ToBlockIndex(this ivec3 blockPos)
        => blockPos.z + blockPos.y * ChunkSize + blockPos.x * ChunkStep;

    public static ivec3 WorldToChunkPosition(this dvec3 worldPosition) {
        var floored = dvec3.Floor(worldPosition / ChunkSize);
        return new((int)floored.x, (int)floored.y, (int)floored.z);
    }

    public static ivec3 BlockToChunkPosition(this ivec3 worldPosition)
        => WorldToChunkPosition(worldPosition);

    public static ivec3 WorldToBlockPosition(this dvec3 worldPosition) {
        var floored = dvec3.Floor(worldPosition);
        return new((int)floored.x, (int)floored.y, (int)floored.z);
    }

    public static dvec3 ChunkToWorldPosition(this ivec3 chunkPosition)
        => (dvec3)chunkPosition * ChunkSize;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int Loop(this int i, int size) {
        int modulo = i % size;

        if (modulo < 0)
            modulo = size + modulo;

        return modulo;
    }

    public static ivec3 Loop(this ivec3 pos, int size)
        => new(pos.x.Loop(size), pos.y.Loop(size), pos.z.Loop(size));
    public static ivec3 Loop(this ivec3 pos, ivec3 size)
        => new(pos.x.Loop(size.x), pos.y.Loop(size.y), pos.z.Loop(size.z));
}
