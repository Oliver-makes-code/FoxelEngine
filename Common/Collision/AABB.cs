using GlmSharp;
using Voxel.Common.Util;
using Voxel.Common.World.Views;

namespace Voxel.Common.Collision;

public record struct AABB(dvec3 Min, dvec3 Max) {
    public bool CollidesWith(BlockView world) {
        var min = Min.WorldToBlockPosition();
        var max = Max.WorldToBlockPosition();
        foreach (var pos in Iteration.Cubic(min, max))
            if (world.GetBlock(pos).IsSolidBlock)
                return true;
        return false;
    }
}
