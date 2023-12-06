using System.Diagnostics.CodeAnalysis;
using GlmSharp;
using Voxel.Common.Tile;
using Voxel.Common.Util;
using Voxel.Common.World.Generation;
using Voxel.Common.World.Storage;
using Voxel.Common.World.Views;

namespace Voxel.Common.World;

public class VoxelWorld : BlockView {

    private readonly Dictionary<ivec3, Chunk> Chunks = new();

    public bool TryGetChunkRaw(ivec3 chunkPos, [NotNullWhen(true)] out Chunk? chunk) => Chunks.TryGetValue(chunkPos, out chunk);
    public bool TryGetChunk(dvec3 worldPosition, [NotNullWhen(true)] out Chunk? chunk) => TryGetChunkRaw(worldPosition.WorldToChunkPosition(), out chunk);
    public bool TryGetChunk(ivec3 blockPosition, [NotNullWhen(true)] out Chunk? chunk) => TryGetChunkRaw(blockPosition.BlockToChunkPosition(), out chunk);

    public bool IsChunkLoadedRaw(ivec3 chunkPos) => Chunks.ContainsKey(chunkPos);


    /// <summary>
    /// Gets or creates a chunk at a given position.
    ///
    /// If the chunk did not exist, the new chunk is empty.
    ///
    /// Ideally, should only be used by terrain generator or when client gets packets from server.
    /// </summary>
    /// <param name="chunkPosition">The chunk-space position of the chunk we're trying to get or create.</param>
    /// <returns></returns>
    public Chunk GetOrCreateChunk(ivec3 chunkPosition) {
        if (Chunks.TryGetValue(chunkPosition, out var chunk))
            return chunk;

        chunk = new(chunkPosition, this);
        SimpleGenerator.GenerateChunk(chunk);

        Chunks[chunkPosition] = chunk;
        return chunk;
    }

    internal void UnloadChunk(ivec3 chunkPosition) {
        Chunks.Remove(chunkPosition);
    }

    internal ChunkView GetOrCreateChunkView(ivec3 chunkPosition) {
        var chunk = GetOrCreateChunk(chunkPosition);
        return new(chunk);
    }

    public void SetBlock(ivec3 position, Block block) {
        var chunkPos = position.BlockToChunkPosition();
        if (!TryGetChunkRaw(chunkPos, out var chunk))
            return;

        var lPos = position - chunk.WorldPosition;
        chunk.SetBlock(lPos, block);
    }

    public Block GetBlock(ivec3 position) {
        var chunkPos = position.BlockToChunkPosition();
        if (!TryGetChunkRaw(chunkPos, out var chunk))
            return Blocks.Air;

        var lPos = position - chunk.WorldPosition;
        return chunk.GetBlock(lPos);
    }

    public void Tick() {
        
    }
    
    public void Dispose() {
        foreach (var chunk in Chunks.Values)
            chunk.Dispose();
    }
}
