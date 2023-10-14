using GlmSharp;
using Voxel.Common.Tile;
using Voxel.Common.Util;
using Voxel.Common.World;

namespace Voxel.Client.World;

public class ClientWorld : VoxelWorld {

    public ClientWorld() {


        foreach (var cpos in Iteration.Cubic(-2, 3))
            GetOrCreateChunk(cpos);


        //SetBlock(ivec3.Zero, Blocks.Stone);
        //SetBlock(new ivec3(1, -1, 0), Blocks.Stone);
        //SetBlock(new ivec3(1, -1, -1), Blocks.Stone);
        //SetBlock(new ivec3(1, 0, -1), Blocks.Stone);
    }

}
