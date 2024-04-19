using GlmSharp;
using Voxel.Client.Rendering.Utils;

namespace Voxel.Client.Rendering;

public class Camera {
    public dvec3 position = new(0, 10, 0);
    public dvec2 rotationVec = dvec2.Zero;

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

    /// <summary>
    /// View frustum of the camera.
    /// </summary>
    public Frustum Frustum { get; private set; }


    public void UpdateFrustum()
        => Frustum = new(this);
}
