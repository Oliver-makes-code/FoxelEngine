using System.Diagnostics.Contracts;
using GlmSharp;
using Voxel.Common.Util;
using Voxel.Common.World.Views;

namespace Voxel.Common.Collision;

public readonly struct AABB {
    public readonly dvec3 Min;
    public readonly dvec3 Max;

    public AABB(dvec3 a, dvec3 b) {
        Min = new(
            Math.Min(a.x, b.x),
            Math.Min(a.y, b.y),
            Math.Min(a.z, b.z)
        );
        Max = new(
            Math.Max(a.x, b.x),
            Math.Max(a.y, b.y),
            Math.Max(a.z, b.z)
        );
    }
    
    [Pure]
    public bool CollidesWith(BlockView world) {
        var min = Min.WorldToBlockPosition();
        var max = (ivec3)dvec3.Ceiling(Max);
        return Iteration.Cubic(min, max).Any(pos => world.GetBlock(pos).IsSolidBlock);
    }

    [Pure]
    public dvec3 MoveAndSlide(BlockView world, dvec3 delta) {
        var newDelta = new dvec3();

        if (!CollideOnAxis(world, delta * dvec3.UnitX))
            newDelta += delta * dvec3.UnitX;
        if (!CollideOnAxis(world, delta * dvec3.UnitY))
            newDelta += delta * dvec3.UnitY;
        if (!CollideOnAxis(world, delta * dvec3.UnitZ))
            newDelta += delta * dvec3.UnitZ;

        return newDelta;
    }
    
    [Pure]
    private bool CollideOnAxis(BlockView world, dvec3 axisDelta) {
        var box = new AABB(Min + axisDelta, Max + axisDelta);
        return box.CollidesWith(world);
    }
}
