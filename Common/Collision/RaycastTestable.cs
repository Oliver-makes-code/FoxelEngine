namespace Voxel.Common.Collision;

public interface RaycastTestable {
    bool Raycast(Ray ray, out RayCastHit hit);
}
