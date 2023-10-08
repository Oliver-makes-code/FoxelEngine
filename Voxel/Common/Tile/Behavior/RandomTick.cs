using System;
using Voxel.Common.World;
using GlmSharp;

namespace Voxel.Common.Tile.Behavior; 

public interface RandomTickable {
    public void RandomTick(VoxelWorld world, Random rng, ivec3 tilePos);
}
