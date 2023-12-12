using GlmSharp;

namespace Voxel.Common.Collision;

/// <summary>
/// Represents the data returned by a raycast operation.
/// </summary>
public struct RaycastHit {
    public dvec3 point;
    public dvec3 normal;
    public bool startedInside;
    public double distance;
}
