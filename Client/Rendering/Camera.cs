using GlmSharp;

namespace Voxel.Client.Rendering;

public class Camera {
    public dvec3 position = new(0, 10, 0);
    public vec2 rotationVec = vec2.Zero;
    
    public quat rotationY => quat.Identity.Rotated(rotationVec.y, new(0, 1, 0));
    public quat rotation => rotationY.Rotated(rotationVec.x, new(1,0,0));

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
    public float nearClip = 0.1f;

    /// <summary>
    /// Far clip plane of camera.
    /// </summary>
    public float farClip = 500;
}
