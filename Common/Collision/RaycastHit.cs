using GlmSharp;

namespace Foxel.Common.Collision;

/// <summary>
/// Represents the data returned by a raycast operation.
/// </summary>
public struct RaycastHit {
    public dvec3 point;
    public dvec3 normal;
    public bool startedInside;
    public double distance;
}

public struct BlockRaycastHit {
    public dvec3 point;
    public dvec3 normal;
    public bool startedInside;
    public double distance;
    public ivec3 blockPos;

    public BlockRaycastHit() {}
    
    public BlockRaycastHit(RaycastHit hit) {
        point = hit.point;
        normal = hit.normal;
        startedInside = hit.startedInside;
        distance = hit.distance;
    }

    public static implicit operator RaycastHit(BlockRaycastHit hit)
        => new() {
            point = hit.point,
            normal = hit.normal,
            startedInside = hit.startedInside,
            distance = hit.distance
        };
}
