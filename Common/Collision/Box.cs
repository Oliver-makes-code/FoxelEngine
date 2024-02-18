using GlmSharp;
using Voxel.Common.Util;

namespace Voxel.Common.Collision;

public struct Box : RaycastTestable {
    public dvec3 min;
    public dvec3 max;

    public dvec3 center => (min + max) * 0.5;
    public dvec3 size => max - min;
    public dvec3 extents => max - center;

    public Box(dvec3 a, dvec3 b) {
        min = dvec3.Min(a, b);
        max = dvec3.Max(a, b);
    }

    public Box Encapsulate(dvec3 point)
        => new() {
            min = dvec3.Min(min, point), max = dvec3.Max(max, point)
        };

    public Box Encapsulate(Box other)
        => new() {
            min = dvec3.Min(min, other.min), max = dvec3.Max(max, other.max)
        };

    public Box Translated(dvec3 vec)
        => new() {
            min = min + vec, max = max + vec
        };

    public Box Expanded(dvec3 size)
        => new() {
            min = min - size * 0.5, max = max + size * 0.5,
        };

    public Box Expanded(Box box)
        => Expanded(box.size);

    public Box Expanded(double size)
        => new() {
            min = min - size * 0.5, max = max + size * 0.5,
        };

    public Box Including(dvec3 point) => new() {
        min = dvec3.Min(min, point), max = dvec3.Max(max, point)
    };

    /// <summary>
    /// Checks if two boxes intersect.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Intersects(Box other)
        => (min < other.max & max > other.min).All;


    /// <summary>
    /// Checks if a point is inside the box.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool Contains(dvec3 point)
        => (point < min & point > max).All;


    /// <summary>
    /// Returns the closest point inside the box.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public dvec3 ClosestPointInside(dvec3 point)
        => dvec3.Clamp(point, min, max);


    /// <summary>
    /// Tests a ray against the surface of an box.
    /// </summary>
    /// <returns>True if hit, false otherwise</returns>
    public bool Raycast(Ray ray, out RaycastHit hit) {

        if (Contains(ray.Position)) {
            hit = new() {
                point = ray.Position, normal = -ray.Direction, distance = 0, startedInside = true,
            };
            return true;
        }

        var c = center;

        hit = new();

        var mm = (min - ray.Position) * ray.InverseDirection;
        var mx = (max - ray.Position) * ray.InverseDirection;

        var tMin = dvec3.Min(mm, mx).MaxElement;
        var tmax = dvec3.Max(mm, mx).MinElement;

        hit.point = ray.GetPoint(tMin);
        hit.distance = tmax >= tMin ? tMin : float.PositiveInfinity;
        hit.normal = dvec3.UnitY;

        if (tmax < tMin)
            return false;

        var local = ((hit.point - c) / size);
        local /= dvec3.Abs(local).MaxElement;

        hit.normal = dvec3.Truncate(local).Normalized; //TODO - Fix corners.

        return tmax >= tMin;
    }

    /// <summary>
    /// Tests one AABB moving until it hits another box.
    /// </summary>
    /// <returns>True if hit, false otherwise</returns>
    public bool Raycast(Box box, dvec3 dir, out RaycastHit hit) {
        var modified = this.Expanded(box);
        return modified.Raycast(new Ray(box.center, dir), out hit);
    }

    public static Box FromPosSize(dvec3 position, dvec3 size) => new() {
        min = position - size * 0.5, max = position + size * 0.5
    };


    /// <summary>
    /// Tests if the box is intersecting or in front of a given plane.
    /// </summary>
    public bool TestAgainstPlane(Plane p) {
        p.Flip();
        var c = center;
        var e = extents;

        var r = (e * dvec3.Abs(p.normal)).Sum;
        var s = -dvec3.Dot(p.normal, c) - p.distance;

        return -r < s;
    }
}
