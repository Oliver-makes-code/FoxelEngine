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
        
        // Console.WriteLine(delta);

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
        
        delta.x = GetMinimumDistanceForAxis(other, delta * dvec3.UnitX);
        delta.y = GetMinimumDistanceForAxis(other, delta * dvec3.UnitY);
        delta.z = GetMinimumDistanceForAxis(other, delta * dvec3.UnitZ);
        
        // Console.WriteLine(delta);

        return delta;
    }

    private double GetMinimumDistanceForAxis(AABB other, dvec3 axisDelta) {
        double delta = axisDelta.Sum;
        var axis = dvec3.Sign(axisDelta);
        
        if (delta == 0)
            return 0;
        
        double s, o;
        
        if (delta > 0) {
            s = (Max * axis).Sum;
            o = (other.Min * axis).Sum;

            if (s < o && s + delta > o) {
                double dist = s - o;
                if (dist <= 0.02)
                    return 0;
                return dist * Math.Sign(delta);
            }
            return delta;
        }
        
        s = (Min * axis).Sum;
        o = (other.Max * axis).Sum;
        
        if (s > o && s + delta < o) {
            double dist = o - s;
        
            if (dist <= 0.02)
                return 0;
            
            return (s - o) * Math.Sign(delta);
        }
        
        return delta;
    }
}
