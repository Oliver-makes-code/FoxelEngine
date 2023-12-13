using GlmSharp;
using Voxel.Common.Content;
using Voxel.Common.Tile;
using Voxel.Common.Util;
using Voxel.Common.World.Storage;

namespace Voxel.Common.World.Views;

/// <summary>
/// Creates a read-only snapshot of a VoxelWorld, copying the chunks inside it in a specified range.
/// </summary>
public class SnapshotView : BlockView {

    private readonly Dictionary<ivec3, ChunkStorage> Storages = new();

    public void Update(VoxelWorld world, ivec3[] positions) {

        //Clear old entries.
        foreach (var value in Storages.Values)
            value.Dispose();
        Storages.Clear();

        //Copy in new ones.
        foreach (var position in positions)
            if (world.TryGetChunkRaw(position, out var chunk))
                Storages[position] = chunk.CopyStorage();
    }

    public void SetBlock(ivec3 position, Block block) {}

    public Block GetBlock(ivec3 position) {
        var chunkPos = position.BlockToChunkPosition();

        if (!Storages.TryGetValue(chunkPos, out var storage))
            return MainContentPack.Instance.Air;

        var localPos = position - chunkPos;
        return storage[localPos];
    }

    public void Dispose() {
        foreach (var value in Storages.Values)
            value.Dispose();
    }
}
