using GlmSharp;
using Voxel.Common.World;
using Voxel.Common.World.Generation;

namespace Voxel.Common.Server.World;

public class ServerWorld : VoxelWorld {
    protected override Chunk CreateChunk(ivec3 pos) {
        var baseChunk = base.CreateChunk(pos);
        SimpleGenerator.GenerateChunk(baseChunk);
        return baseChunk;
    }
}
