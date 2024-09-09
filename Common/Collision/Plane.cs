using GlmSharp;
using Foxel.Common.Collision;
using Foxel.Core.Util;

namespace Foxel.Common.Util;

public struct Plane {
    public dvec3 normal;
    public double distance;

    public Plane(dvec3 point, dvec3 normal) {
        this.normal = normal.NormalizedSafe;
        distance = -dvec3.Dot(point, this.normal);
    }

    public Plane(vec3 point, vec3 normal) : this((dvec3)point, (dvec3)normal) {
    }

    public Plane(vec3 a, vec3 b, vec3 c) {
        normal = (dvec3.Cross(b - a, c - a)).NormalizedSafe;
        distance = -dvec3.Dot(normal, a);
    }

    public Plane(dvec3 a, dvec3 b, dvec3 c) {
        normal = (dvec3.Cross(b - a, c - a)).NormalizedSafe;
        distance = -dvec3.Dot(normal, a);
    }
    
    public void Flip() {
        normal = -normal;
        distance = -distance;
    }

    public double Distance(dvec3 point) => dvec3.Dot(normal, point) + distance;

    public dvec3 ClosestPoint(dvec3 point) => point - normal * Distance(point);

    public bool GetSide(dvec3 point) => Distance(point) >= 0;

    public bool SameSide(dvec3 a, dvec3 b) => GetSide(a) == GetSide(b);

    public readonly bool Raycast(Ray ray, out RaycastHit hit) {
        double vdot = dvec3.Dot(ray.Direction, normal);
        double ndot = (-vdot) - distance;

        if (MathHelper.Approximately(vdot, 0)) {
            hit = default;
            return false;
        }

        hit = default;
        hit.distance = ndot / vdot; //TODO: Is this accurate?...
        hit.point = ray.GetPoint(hit.distance);
        
        return true;
    }
}
