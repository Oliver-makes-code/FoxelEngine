using GlmSharp;
using Voxel.Common.Collision;
using Voxel.Common.World;

namespace Voxel.Common.Entity;

public abstract class Entity {
    public abstract float eyeHeight { get; }
    public abstract AABB boundingBox { get; }
    public vec3 position = new(0, 0, 0);

    public abstract void Tick(VoxelWorld voxelWorld);
}
