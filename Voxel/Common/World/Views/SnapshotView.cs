using System.Collections.Generic;
using GlmSharp;
using Voxel.Common.Tile;
using Voxel.Common.Util;
using Voxel.Common.World.Storage;

namespace Voxel.Common.World.Views;

/// <summary>
/// Creates a read-only snapshot of a VoxelWorld, copying the chunks inside it in a specified range.
/// </summary>
public class SnapshotView : IBlockView {

    private readonly Dictionary<ivec3, ChunkStorage> _storages = new();

    public void Update(VoxelWorld world, ivec3[] positions) {

        //Clear old entries.
        foreach (var value in _storages.Values)
            value.Dispose();
        _storages.Clear();

        //Copy in new ones.
        foreach (var position in positions) {
            if (world.TryGetChunkRaw(position, out var chunk))
                _storages[position] = chunk.CopyStorage();
        }
    }

    public void SetBlock(ivec3 position, Block block) {}

    public Block GetBlock(ivec3 position) {
        var chunkPos = position.BlockToChunkPosition();

        if (!_storages.TryGetValue(chunkPos, out var storage))
            return Blocks.Air;

        var localPos = position - chunkPos;
        return storage[localPos];
    }

    public void Dispose() {
        foreach (var value in _storages.Values)
            value.Dispose();
    }
}
