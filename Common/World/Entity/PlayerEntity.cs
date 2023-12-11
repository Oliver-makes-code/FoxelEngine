using GlmSharp;
using Voxel.Common.Collision;

namespace Voxel.Common.World.Entity;

public class PlayerEntity : Entity {
    public override float eyeHeight { get; } = 1.62f;
    public override AABB boundingBox { get; } = AABB.FromPosSize(new(0, 0, 0), new dvec3(1, 2, 1) * 0.95);
}
