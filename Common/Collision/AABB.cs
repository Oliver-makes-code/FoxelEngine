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
    public double MoveAndSlide(BlockView world, dvec3 delta, out dvec3 normal) {
        normal = new();
        
        var min = (ivec3)dvec3.Floor(Min) - new ivec3(1,1,1);
        var max = (ivec3)dvec3.Ceiling(Max) + new ivec3(1,1,1);

        double minPercent = 1;

        foreach (var pos in Iteration.Cubic(min, max))
            if (world.GetBlock(pos).IsSolidBlock) {
                double slide = SlideWith(new(pos, pos + new ivec3(1)), delta, out var locNormal);
                if (slide >= minPercent)
                    continue;
                normal = locNormal;
                minPercent = slide;
            }

        return minPercent;
    }

    private bool CollidesWith(AABB other) 
        => Min.x < other.Max.x && 
            Max.x > other.Min.x &&
            Min.y < other.Max.y &&
            Max.y > other.Min.y &&
            Min.z < other.Max.z &&
            Max.z > other.Min.z;

    private bool CollidesWithOnAxis(AABB other, int axis)
        => Min[axis] < other.Max[axis] &&
            Max[axis] > other.Min[axis];
    
    public double SlideWith(AABB other, dvec3 delta, out dvec3 normal) {
        normal = new();
        if (!new AABB(Min + delta, Max + delta).CollidesWith(other))
            return 1;
        
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
                enter[i] = Min[i] - other.Max[i];
                exit[i] = Max[i] - other.Min[i];
            } else if (CollidesWithOnAxis(other, i)) {
                enter[i] = 0;
                exit[i] = double.MaxValue;
            } else {
                enter[i] = double.MaxValue;
                exit[i] = -1;
            }
        }
        
        dvec3
            enterDiv = enter / length,
            exitDiv = exit / length;

        double
            enterDivMax = double.MinValue,
            exitDivMin = exitDiv.Min();
        
        int maxDir = 0;

        for (int i = 0; i < 3; i++) {
            if (enterDivMax >= enterDiv[i])
                continue;
            enterDivMax = enterDiv[i];
            maxDir = i;
        }

        normal[maxDir] = -Math.Sign(delta[maxDir]);
        
        if (enterDivMax > exitDivMin)
            return 0;
        if (enterDivMax > 1)
            return 0;
        if (enterDivMax < 0 || exitDivMin < 0)
            return 0;

        return enterDivMax;
    }
}
