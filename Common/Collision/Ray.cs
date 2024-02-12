using GlmSharp;

namespace Voxel.Common.Collision;

public struct Ray {
    public readonly dvec3 Position;
    public readonly dvec3 Direction;
    public readonly dvec3 InverseDirection;

    public Ray(dvec3 pos, dvec3 dir) {
        Position = pos;
        Direction = dir.Normalized;
        InverseDirection = 1 / Direction;
    }

    public dvec3 GetPoint(float t)
        => Position + Direction * t;
    public readonly dvec3 GetPoint(double t)
        => Position + Direction * t;
}

public struct RaySegment {
    public readonly Ray Ray;
    public readonly double Distance;
    public readonly dvec3 Position => Ray.Position;
    public readonly dvec3 Direction => Ray.Direction;
    public readonly dvec3 Delta => Direction.Normalized * Distance;
    public readonly dvec3 Dest => Position + Delta;

    public RaySegment(Ray ray, double distance) {
        Ray = ray;
        Distance = distance;
    }


    public RaySegment(dvec3 pos, dvec3 body) : this(new(pos, body.Normalized), body.Length) {}

    public RaySegment(dvec3 pos, dvec3 dir, double distance) : this(new(pos, dir), distance) {}

    public static implicit operator Ray(RaySegment segment)
        => segment.Ray;
}
