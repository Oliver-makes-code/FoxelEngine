using System;
using System.Runtime.InteropServices;
using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.VertexTypes;

namespace Voxel.Client.Rendering.Debug;

public class DebugRenderer : Renderer {
    private const int BatchSize = 8192 * 4;
    private static DebugRenderer Instance;

    public readonly Pipeline DebugPipeline;
    private readonly DeviceBuffer vertexBuffer;

    private readonly DebugVertex[] DebugVertices = new DebugVertex[BatchSize];
    private int vertexIndex = 0;

    private vec4 color = vec4.Ones;
    private mat4 matrix = mat4.Identity;

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
        Flush();
    }

    private void Flush() {
        if (vertexIndex == 0)
            return;

        CommandList.UpdateBuffer(vertexBuffer, 0, DebugVertices.AsSpan(0, vertexIndex));
        SetPipeline(DebugPipeline);

        CommandList.SetGraphicsResourceSet(0, Client.GameRenderer.CameraStateManager.CameraResourceSet);

        CommandList.SetVertexBuffer(0, vertexBuffer);
        CommandList.Draw((uint)vertexIndex);

        vertexIndex = 0;

        //RestoreLastPipeline();
    }

    public override void Dispose() {
        DebugPipeline.Dispose();
    }

    private void AddPoint(dvec3 pos) {
        Instance.DebugVertices[Instance.vertexIndex++] = new DebugVertex {
            color = color, position = (matrix * new vec4((vec3)(pos - Client.GameRenderer.MainCamera.position), 1)).xyz
        };

        if (Instance.vertexIndex >= BatchSize)
            Instance.Flush();
    }

    public static void SetColor(vec4 color) {
        Instance.color = color;
    }

    public static void SetMatrix() => SetMatrix(mat4.Identity);
    public static void SetMatrix(mat4 matrix) {
        Instance.matrix = matrix;
    }

    public static void DrawLine(dvec3 a, dvec3 b) {
        Instance.AddPoint(a);
        Instance.AddPoint(b);
    }

    public static void DrawLines(params dvec3[] lines) {
        for (var i = 0; i < lines.Length - 1; i++) {
            Instance.AddPoint(lines[i]);
            Instance.AddPoint(lines[i + 1]);
        }
    }

    public static void DrawLinesLoop(params dvec3[] lines) {
        for (var i = 0; i < lines.Length; i++) {
            Instance.AddPoint(lines[i]);
            Instance.AddPoint(lines[(i + 1) % lines.Length]);
        }
    }

    public static void DrawCube(dvec3 min, dvec3 max, float expansion = 0) {
        var realMin = dvec3.Min(min, max);
        var realMax = dvec3.Max(min, max);

        var center = dvec3.Lerp(min, max, 0.5d);
        var separation = (realMax - center).Normalized;

        realMin -= separation * expansion;
        realMax += separation * expansion;

        DrawLinesLoop(
            new dvec3(realMin.x, realMin.y, realMin.z),
            new dvec3(realMin.x, realMin.y, realMax.z),
            new dvec3(realMax.x, realMin.y, realMax.z),
            new dvec3(realMax.x, realMin.y, realMin.z)
        );
        DrawLinesLoop(
            new dvec3(realMin.x, realMax.y, realMin.z),
            new dvec3(realMin.x, realMax.y, realMax.z),
            new dvec3(realMax.x, realMax.y, realMax.z),
            new dvec3(realMax.x, realMax.y, realMin.z)
        );

        DrawLine(new dvec3(realMin.x, realMin.y, realMin.z), new dvec3(realMin.x, realMax.y, realMin.z));
        DrawLine(new dvec3(realMin.x, realMin.y, realMax.z), new dvec3(realMin.x, realMax.y, realMax.z));
        DrawLine(new dvec3(realMax.x, realMin.y, realMax.z), new dvec3(realMax.x, realMax.y, realMax.z));
        DrawLine(new dvec3(realMax.x, realMin.y, realMin.z), new dvec3(realMax.x, realMax.y, realMin.z));
    }
}