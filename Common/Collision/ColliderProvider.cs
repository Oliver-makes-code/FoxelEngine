namespace Foxel.Common.Collision;

/// <summary>
/// Provides colliders that intersect with a given box.
/// </summary>
public interface ColliderProvider {
    List<Box> GatherColliders(Box box);
}
