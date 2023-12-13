using GlmSharp;
using Voxel.Common.World;
using Voxel.Common.World.Storage;

namespace Voxel.Common.Server.World;

public class ServerChunk : Chunk {
    private readonly ServerWorld ServerWorld;

    public ServerChunk(ivec3 chunkPosition, ServerWorld world, ChunkStorage? storage = null) : base(chunkPosition, world, storage) {
        ServerWorld = world;
    }
}
