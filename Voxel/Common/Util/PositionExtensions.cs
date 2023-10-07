using System.Runtime.CompilerServices;
using GlmSharp;

namespace Voxel.Common.Util;

public static class PositionExtensions {

    public const int CHUNK_SIZE = 32;
    public const int CHUNK_STEP = CHUNK_SIZE * CHUNK_SIZE;
    public const int CHUNK_CAPACITY = CHUNK_STEP * CHUNK_SIZE;

    public static int ToBlockIndex(this ivec3 blockPos) => blockPos.z + blockPos.y * CHUNK_SIZE + blockPos.x * CHUNK_STEP;

    public static ivec3 WorldToChunkPosition(this dvec3 worldPosition) {
        var floored = dvec3.Floor(worldPosition / CHUNK_SIZE);
        return new((int)floored.x, (int)floored.y, (int)floored.z);
    }

    public static ivec3 BlockToChunkPosition(this ivec3 worldPosition) => WorldToChunkPosition(worldPosition);

    public static ivec3 WorldToBlockPosition(this dvec3 worldPosition) {
        var floored = dvec3.Floor(worldPosition);
        return new((int)floored.x, (int)floored.y, (int)floored.z);
    }

    public static dvec3 ChunkToWorldPosition(this ivec3 chunkPosition) => (dvec3)chunkPosition * CHUNK_SIZE;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int Loop(this int i, int size) {
        var modulo = i % size;

        if (modulo < 0)
            modulo = size + modulo;

        return modulo;
    }

    public static ivec3 Loop(this ivec3 pos, int size) => new(pos.x.Loop(size), pos.y.Loop(size), pos.z.Loop(size));
    public static ivec3 Loop(this ivec3 pos, ivec3 size) => new(pos.x.Loop(size.x), pos.y.Loop(size.y), pos.z.Loop(size.z));


    public static int Packed(this vec4 color) => ((ivec4)(color * 255)).Packed();

    public static int Packed(this ivec4 color) => (color.r & 255) | (color.g & 255) << 8 | (color.b & 255) << 16 | (color.a & 255) << 24;

    public static vec4 ColorUnpacked(this int value) {
        return new vec4(
            (value & 255) / 255.0f,
            ((value >> 8) & 255) / 255.0f,
            ((value >> 16) & 255) / 255.0f,
            ((value >> 24) & 255) / 255.0f
        );
    }
}
