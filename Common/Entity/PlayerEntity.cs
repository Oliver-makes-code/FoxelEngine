using Voxel.Common.Collision;
using Voxel.Common.World;

namespace Voxel.Common.Entity;

public class PlayerEntity : Entity {
    public override float eyeHeight { get; } = 1.62f;
    public override AABB boundingBox { get; } = new(new(0,0,0), new(0,0,0));

    public override void Tick(VoxelWorld voxelWorld) {
        //TODO!
    }
}
