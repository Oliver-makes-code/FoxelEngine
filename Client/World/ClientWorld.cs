using GlmSharp;
using Foxel.Common.World;

namespace Foxel.Client.World;

public class ClientWorld : VoxelWorld {
    public override bool IsChunkLoadedRaw(ivec3 chunkPos)
        => TryGetChunkRaw(chunkPos, out var c) && c is ClientChunk cc && cc.isFilled;

    protected override Chunk CreateChunk(ivec3 pos)
        => new ClientChunk(pos, this);
}
