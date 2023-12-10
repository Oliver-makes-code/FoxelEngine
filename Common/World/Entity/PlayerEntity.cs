using Voxel.Common.Collision;

namespace Voxel.Common.World.Entity;

public class PlayerEntity : Entity {
    public override float eyeHeight { get; } = 1.62f;
    public override AABB boundingBox { get; } = new(new(0, 0, 0), new(0, 0, 0));
}
