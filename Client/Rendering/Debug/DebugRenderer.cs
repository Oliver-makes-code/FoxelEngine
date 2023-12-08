using System;
using System.Runtime.InteropServices;
using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.VertexTypes;

namespace Voxel.Client.Rendering.Debug;

public class DebugRenderer : Renderer {
    private const int BatchSize = 2048;
    private static DebugRenderer Instance;

    public readonly Pipeline DebugPipeline;
    private readonly DeviceBuffer vertexBuffer;

    private readonly DebugVertex[] DebugVertices = new DebugVertex[BatchSize];
    private int vertexIndex = 0;

    public DebugRenderer(VoxelClient client) : base(client) {
        Instance = this;

        if (!client.RenderSystem.ShaderManager.GetShaders("shaders/debug", out var shaders))
            throw new("Shaders not present.");

        DebugPipeline = ResourceFactory.CreateGraphicsPipeline(new() {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new() {
                DepthComparison = ComparisonKind.LessEqual, DepthTestEnabled = true, DepthWriteEnabled = true,
            },
            Outputs = RenderSystem.GraphicsDevice.SwapchainFramebuffer.OutputDescription,
            PrimitiveTopology = PrimitiveTopology.LineList,
            RasterizerState = new() {
                CullMode = FaceCullMode.None, DepthClipEnabled = false, FillMode = PolygonFillMode.Wireframe, ScissorTestEnabled = false,
            },
            ResourceLayouts = new[] {
                Client.GameRenderer.CameraStateManager.CameraResourceLayout,
            },
            ShaderSet = new() {
                VertexLayouts = new[] {
                    DebugVertex.Layout
                },
                Shaders = shaders
            }
        });


        vertexBuffer = ResourceFactory.CreateBuffer(new BufferDescription {
            Usage = BufferUsage.Dynamic | BufferUsage.VertexBuffer, SizeInBytes = (uint)Marshal.SizeOf<DebugVertex>() * BatchSize
        });
    }
    public override void Render(double delta) {

        DrawLine(new() {
                position = new vec3(0, 0, 0) - (vec3)Client.GameRenderer.MainCamera.position, color = new vec4(1, 1, 1, 1)
            },
            new() {
                position = new vec3(0, 1000, 0) - (vec3)Client.GameRenderer.MainCamera.position, color = new vec4(1, 1, 1, 1)
            }
        );

        Flush();
    }

    private void Flush() {
        if (vertexIndex == 0)
            return;

        CommandList.UpdateBuffer(vertexBuffer, 0, DebugVertices.AsSpan(0, vertexIndex));
        CommandList.SetPipeline(DebugPipeline);

        CommandList.SetGraphicsResourceSet(0, Client.GameRenderer.CameraStateManager.CameraResourceSet);

        CommandList.SetVertexBuffer(0, vertexBuffer);
        CommandList.Draw((uint)vertexIndex);

        vertexIndex = 0;
    }

    public override void Dispose() {
        DebugPipeline.Dispose();
    }


    public static void DrawLine(DebugVertex a, DebugVertex b) {
        Instance.DebugVertices[Instance.vertexIndex++] = a;
        Instance.DebugVertices[Instance.vertexIndex++] = b;

        if (Instance.vertexIndex >= BatchSize) {
            Instance.Flush();
        }
    }

    /*public static void DrawCube(DebugVertex min, DebugVertex max) {
        DebugVertices.Add(a);
        DebugVertices.Add(b);
    }*/
}
