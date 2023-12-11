using GlmSharp;

namespace Voxel.Common.Collision;

public struct Ray {
    public readonly dvec3 position;
    public readonly dvec3 direction;
    public readonly dvec3 inverseDirection;

    public Ray(dvec3 pos, dvec3 dir) {
        position = pos;
        direction = dir.Normalized;
        inverseDirection = 1 / direction;
    }

    public dvec3 GetPoint(float t)
        => position + direction * t;
    public dvec3 GetPoint(double t)
        => position + direction * t;
}
