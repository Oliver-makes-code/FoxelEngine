using GlmSharp;

namespace Voxel.Client.Rendering;

public class Camera {
    public dvec3 position = dvec3.Zero;
    public quat rotation = quat.Identity;

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
