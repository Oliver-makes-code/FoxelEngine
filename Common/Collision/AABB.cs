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
        var min = (ivec3)dvec3.Floor(Min);
        var max = (ivec3)dvec3.Ceiling(Max);
        foreach (var pos in Iteration.Cubic(min, max))
            if (world.GetBlock(pos).IsSolidBlock)
                return true;
        return false;
    }

    [Pure]
    public dvec3 MoveAndSlide(BlockView world, dvec3 delta) {
        var min = (ivec3)dvec3.Floor(Min) - new ivec3(1,1,1);
        var max = (ivec3)dvec3.Ceiling(Max) + new ivec3(1,1,1);

        foreach (var pos in Iteration.Cubic(min, max))
            if (world.GetBlock(pos).IsSolidBlock)
                delta = GetMinimumDistance(new(pos, pos + new ivec3(1, 1, 1)), delta);

        return delta;
    }

    private bool CollidesWith(AABB other) 
        => Min.x < other.Max.x && 
            Max.x > other.Min.x &&
            Min.y < other.Max.y &&
            Max.y > other.Min.y &&
            Min.z < other.Max.z &&
            Max.z > other.Min.z;

    private dvec3 GetMinimumDistance(AABB other, dvec3 delta) {
        if (!new AABB(Min + delta, Max + delta).CollidesWith(other))
            return delta;

        double d = GetMinimumDistanceForAxis(other, delta);
        
        if (d != 0)
            Console.WriteLine(d);

        return delta;
    }
    
    // TODO: fix method name
    private double GetMinimumDistanceForAxis(AABB other, dvec3 delta) {
        double length = delta.Length;
        if (length == 0)
            return 0;
        
        dvec3
            enter = new(0),
            exit = new(0);

        for (int i = 0; i < 3; i++) {
            if (delta[i] > 0) {
                enter[i] = other.Min[i] - Max[i];
                exit[i] = other.Max[i] - Min[i];
            } else if (delta[i] < 0) {
                exit[i] = other.Min[i] - Max[i];
                enter[i] = other.Max[i] - Min[i];
            }
        }
        
        dvec3
            enterDiv = enter / length,
            exitDiv = exit / length;

        double
            enterDivMax = enterDiv.Max(),
            exitDivMin = exitDiv.Min();
        
        if (enterDivMax > exitDivMin)
            return 0;
        if (enterDivMax > 1 || exitDivMin > 1)
            return 0;
        if (enterDivMax < 0 || exitDivMin < 0)
            return 0;

        return enterDivMax;
    }
}
