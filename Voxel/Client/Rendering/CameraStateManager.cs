using GlmSharp;
using RenderSurface.Rendering;
using Veldrid;
using Voxel.Client.Rendering.Utils;

namespace Voxel.Client.Rendering;

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
        currentCameraPosition = c.position;

        var data = new CameraData();
        data.VPMatrix = mat4.Perspective(c.fovy, c.aspect, c.nearClip, c.farClip).Transposed;
        CameraBuffer.value = data;
    }

    private struct CameraData {
        /// <summary>
        /// View-Projection Matrix.
        /// </summary>
        public mat4 VPMatrix;
    }
}
