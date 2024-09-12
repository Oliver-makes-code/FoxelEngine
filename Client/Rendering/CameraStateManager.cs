using GlmSharp;
using Veldrid;
using Foxel.Core.Rendering;
using Foxel.Core.Util;
using Foxel.Core.Rendering.Resources.Buffer;

namespace Foxel.Client.Rendering;

public class CameraStateManager {

    public static dvec3 currentCameraPosition;

    public readonly RenderSystem RenderSystem;

    public readonly ResourceLayout CameraResourceLayout;
    public readonly ResourceSet CameraResourceSet;

    private readonly GraphicsBuffer<CameraData> CameraBuffer;

    public CameraStateManager(RenderSystem system) {
        RenderSystem = system;

        CameraBuffer = new(RenderSystem, GraphicsBufferType.UniformBuffer, 1);

        CameraResourceLayout = system.ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("Camera Uniform", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        CameraResourceSet = system.ResourceFactory.CreateResourceSet(new(
            CameraResourceLayout,
            CameraBuffer.BaseBuffer
        ));
    }

    public void SetToCamera(Camera c) {
        c.UpdateFrustum();
        currentCameraPosition = c.position;

        var data = new CameraData {
            viewMatrix = ((quat)c.rotationVec.RotationVecToQuat()).ToMat4,
            projectionMatrix = mat4.Perspective(-c.fovy, c.aspect, c.nearClip, c.farClip).Transposed
        };
        CameraBuffer.UpdateDeferred(0, [data]);
    }

    public struct CameraData {
        /// <summary>
        /// View Matrix.
        /// </summary>
        public mat4 viewMatrix;
        public mat4 projectionMatrix;
    }
}
