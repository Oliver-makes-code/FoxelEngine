using GlmSharp;

namespace Voxel.Common.Collision;

/// <summary>
/// Represents the data returned by a raycast operation.
/// </summary>
public struct RayCastHit {
    public dvec3 point;
    public dvec3 normal;
    public double distance;
}
