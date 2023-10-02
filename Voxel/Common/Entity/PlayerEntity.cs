using Voxel.Common.Collision;

namespace Voxel.Common.Entity;

using World = World.World;

public class PlayerEntity : Entity {
    public override float EyeHeight { get; } = 1.62f;
    public override AABB BoundingBox { get; } = new(1, 1.8f);

    public override void Tick(World world) {
        //TODO!
    }
}
