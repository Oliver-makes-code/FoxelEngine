using GlmSharp;
using Foxel.Common.Util;
using Foxel.Common.World.Storage;
using Foxel.Common.World.Content.Blocks;
using Foxel.Common.World.Content.Blocks.State;

namespace Foxel.Common.World.Views;

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

    public void SetBlockState(ivec3 position, BlockState block) {}

    public BlockState GetBlockState(ivec3 position) {
        var chunkPos = position.BlockToChunkPosition();

        if (!Storages.TryGetValue(chunkPos, out var storage))
            return BlockStore.Blocks.Air.Get().DefaultState;

        var localPos = position - chunkPos;
        return storage[localPos];
    }

    public void Dispose() {
        foreach (var value in Storages.Values)
            value.Dispose();
    }
}
