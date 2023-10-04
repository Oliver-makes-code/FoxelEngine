using GlmSharp;
using Voxel.Common.Collision;
using Voxel.Common.World;

namespace Voxel.Common.Entity;

public abstract class Entity {
    public abstract float EyeHeight { get; }
    public abstract AABB BoundingBox { get; }
    public vec3 Position = new(0, 0, 0);

    public abstract void Tick(VoxelWorld voxelWorld);
}
