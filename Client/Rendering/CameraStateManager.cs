using GlmSharp;
using Veldrid;
using Foxel.Client.Rendering.Utils;
using Foxel.Core.Rendering;
using Foxel.Core.Util;

namespace Foxel.Client.Rendering;

public class CameraStateManager {

    public static dvec3 currentCameraPosition;

    public readonly RenderSystem RenderSystem;

    public readonly ResourceLayout CameraResourceLayout;
    public readonly ResourceSet CameraResourceSet;

    private readonly TypedDeviceBuffer<CameraData> CameraBuffer;

    public CameraStateManager(RenderSystem system) {
        RenderSystem = system;

        CameraBuffer = new(
            new() {
                Usage = BufferUsage.UniformBuffer | BufferUsage.Dynamic
            },
            RenderSystem
        );

        CameraResourceLayout = system.ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("Camera Uniform", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        CameraResourceSet = system.ResourceFactory.CreateResourceSet(new(
            CameraResourceLayout,
            CameraBuffer.BackingBuffer
        ));
    }

    public void SetToCamera(Camera c) {
        c.UpdateFrustum();
        currentCameraPosition = c.position;

        var data = new CameraData {
            viewProjectionMatrix = ((quat)c.rotationVec.RotationVecToQuat()).ToMat4 * mat4.Perspective(-c.fovy, c.aspect, c.nearClip, c.farClip).Transposed
        };
        CameraBuffer.value = data;
    }

    public struct CameraData {
        /// <summary>
        /// View-Projection Matrix.
        /// </summary>
        public mat4 viewProjectionMatrix;
    }
}
