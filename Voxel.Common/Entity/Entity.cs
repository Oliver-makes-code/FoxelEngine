using Microsoft.Xna.Framework;
using Voxel.Common.Collision;

namespace Voxel.Common.Entity; 

using World = World.World;

public abstract class Entity {
    public abstract float EyeHeight { get; }
    public abstract AABB BoundingBox { get; }
    public Vector3 Position = new(0, 0, 0);

    public abstract void Tick(World world);
}
