using Microsoft.Xna.Framework;

namespace Voxel.Common.Collision; 

using World = World.World;

public class AABB {
    public readonly float Width;
    public readonly float Height;

    public AABB(float width, float height) {
        Width = width;
        Height = height;
    }

    public bool CollidesWith(World world, Vector3 position) {
        // TODO!
        return false;
    }
}
