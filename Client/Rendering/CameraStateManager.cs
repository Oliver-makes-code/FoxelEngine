using GlmSharp;
using Veldrid;
using Foxel.Client.Rendering.Utils;
using Foxel.Core.Rendering;
using Foxel.Core.Util;
using Foxel.Core.Rendering.Buffer;

namespace Foxel.Client.Rendering;

public class CameraStateManager {

    public static dvec3 currentCameraPosition;

    public readonly RenderSystem RenderSystem;

    public readonly ResourceLayout CameraResourceLayout;
    public readonly ResourceSet CameraResourceSet;

    private readonly TypedGraphicsBuffer<CameraData> CameraBuffer;

    public CameraStateManager(RenderSystem system) {
        RenderSystem = system;

        CameraBuffer = new(RenderSystem, GraphicsBufferUsage.UniformBuffer | GraphicsBufferUsage.Dynamic);
        CameraBuffer.WithCapacity(1);

        CameraResourceLayout = system.ResourceFactory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("Camera Uniform", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        CameraResourceSet = system.ResourceFactory.CreateResourceSet(new(
            CameraResourceLayout,
            CameraBuffer.baseBuffer
        ));
    }

    public void SetToCamera(Camera c) {
        c.UpdateFrustum();
        currentCameraPosition = c.position;

        var data = new CameraData {
            viewMatrix = ((quat)c.rotationVec.RotationVecToQuat()).ToMat4,
            projectionMatrix = mat4.Perspective(-c.fovy, c.aspect, c.nearClip, c.farClip).Transposed
        };
        CameraBuffer.Update(0, [data]);
    }

    public struct CameraData {
        /// <summary>
        /// View Matrix.
        /// </summary>
        public mat4 viewMatrix;
        public mat4 projectionMatrix;
    }
}
