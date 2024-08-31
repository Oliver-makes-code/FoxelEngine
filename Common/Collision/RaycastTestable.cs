namespace Foxel.Common.Collision;

public interface RaycastTestable {
    bool Raycast(Ray ray, out RaycastHit hit);
}
