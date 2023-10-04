using GlmSharp;
using Voxel.Common.World;

namespace Voxel.Common.Collision;

public class AABB {
    public readonly float Width;
    public readonly float Height;

    public AABB(float width, float height) {
        Width = width;
        Height = height;
    }

    public bool CollidesWith(VoxelWorld voxelWorld, vec3 position) {
        // TODO!
        return false;
    }
}
