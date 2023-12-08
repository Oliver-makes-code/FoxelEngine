using GlmSharp;
using Voxel.Common.World;

namespace Voxel.Client.World;

public class ClientWorld : VoxelWorld {
    
    protected override Chunk CreateChunk(ivec3 pos) {
        var c = new ClientChunk(pos, this);
        return c;
    }
}
