using GlmSharp;
using Voxel.Common.Collision;

namespace Voxel.Common.World.Entity.Player;

public class PlayerEntity : LivingEntity {
    public override float eyeHeight { get; } = 1.62f;
    public override Box boundingBox { get; } = Box.FromPosSize(new(0, 0, 0), new dvec3(1, 2, 1) * 0.95);
}
