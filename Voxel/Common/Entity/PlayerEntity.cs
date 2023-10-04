using Voxel.Common.Collision;
using Voxel.Common.World;

namespace Voxel.Common.Entity;

public class PlayerEntity : Entity {
    public override float EyeHeight { get; } = 1.62f;
    public override AABB BoundingBox { get; } = new(1, 1.8f);

    public override void Tick(VoxelWorld voxelWorld) {
        //TODO!
    }
}
