namespace Voxel.Common.Collision;

/// <summary>
/// Provides colliders that intersect with a given AABB.
/// </summary>
public interface ColliderProvider {
    List<Box> GatherColliders(Box box);
}
