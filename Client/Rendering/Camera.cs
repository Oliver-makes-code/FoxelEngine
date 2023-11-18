using System;
using GlmSharp;
using Voxel.Common.Collision;
using Voxel.Common.World;

namespace Voxel.Client.Rendering;

public class Camera {
    public dvec3 oldPosition = new(0, 10, 0);
    public dvec3 position = new(0, 10, 0);
    public vec2 oldRotationVec = vec2.Zero;
    public vec2 rotationVec = vec2.Zero;
    
    public quat rotationY => quat.Identity.Rotated(rotationVec.y, new(0, 1, 0));
    public quat oldRotationY => quat.Identity.Rotated(oldRotationVec.y, new(0, 1, 0));
    public quat rotation => rotationY.Rotated(rotationVec.x, new(1,0,0));
    public quat oldRotation => oldRotationY.Rotated(oldRotationVec.x, new(1,0,0));

    /// <summary>
    /// FOV in degrees from the top to the bottom of the camera.
    /// </summary>
    public float fovy = 80;

    /// <summary>
    /// Aspect ratio of the camera.
    /// </summary>
    public float aspect = 1;


    /// <summary>
    /// Near clip plane of camera.
    /// </summary>
    public float nearClip = 0.05f;

    /// <summary>
    /// Far clip plane of camera.
    /// </summary>
    public float farClip = 500;

    public void MoveAndSlide(VoxelWorld world, dvec3 delta) {
        for (int i = 0; i < 3; i++) {
            if (delta == dvec3.Zero)
                break;
            delta = MoveAndSlideSingle(world, delta);
        }
    }

    private dvec3 MoveAndSlideSingle(VoxelWorld world, dvec3 delta) {
        const double CollisionBackoff = 1/128d;

        double minPercent = new AABB(
            position - new dvec3(0.3, 1.6, 0.3),
            position + new dvec3(0.3, 0.2, 0.3)
        ).MoveAndSlide(world, delta, out var normal);
        
        position += delta * minPercent;
        
        if (minPercent < 1 && minPercent >= 0)
            position += delta.NormalizedSafe * -CollisionBackoff;
        
        double remaining = 1 - minPercent;
        var project = new dvec3(0);

        for (int i = 0; i < 3; i++)
            if (Math.Abs(normal[i]) < 0.0000001)
                project[i] = delta[i] * remaining;

        return project;
    }
}
