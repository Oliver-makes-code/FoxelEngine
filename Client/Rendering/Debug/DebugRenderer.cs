using System;
using System.Runtime.InteropServices;
using GlmSharp;
using Veldrid;
using Foxel.Client.Rendering.VertexTypes;
using Foxel.Common.Collision;
using Foxel.Core.Assets;
using Foxel.Core.Rendering;
using Foxel.Core.Rendering.Buffer;

namespace Foxel.Client.Rendering.Debug;

public class DebugRenderer : Renderer {
    private const int BatchSize = 8192 * 4;
    
    private static DebugRenderer? instance;
    
    private readonly VertexBuffer<DebugVertex> VertexBuffer;

    private readonly VertexConsumer<DebugVertex> Vertices = new();

    private vec4 color = vec4.Ones;
    private mat4 matrix = mat4.Identity;

    public DebugRenderer(VoxelClient client) : base(client, RenderPhase.PreRender) {
        instance = this;

        VertexBuffer = new(RenderSystem);

        WithResourceSet(0, () => Client.gameRenderer!.CameraStateManager.CameraResourceSet);
    }

    public static void SetColor(vec4 color)
        => instance!.color = color;

    public static void SetMatrix()
        => SetMatrix(mat4.Identity);

    public static void SetMatrix(mat4 matrix)
        => instance!.matrix = matrix;

    public static void DrawLine(dvec3 a, dvec3 b) {
        instance!.AddPoint(a);
        instance!.AddPoint(b);
    }

    public static void DrawLines(params dvec3[] lines) {
        for (var i = 0; i < lines.Length - 1; i++) {
            instance!.AddPoint(lines[i]);
            instance!.AddPoint(lines[i + 1]);
        }
    }

    public static void DrawLinesLoop(params dvec3[] lines) {
        for (var i = 0; i < lines.Length; i++) {
            instance!.AddPoint(lines[i]);
            instance!.AddPoint(lines[(i + 1) % lines.Length]);
        }
    }

    public static void DrawCube(dvec3 min, dvec3 max, float expansion = 0) {
        var realMin = dvec3.Min(min, max);
        var realMax = dvec3.Max(min, max);

        var center = dvec3.Lerp(min, max, 0.5d);
        var separation = (realMax - center).Normalized;

        realMin -= separation * expansion;
        realMax += separation * expansion;

        DrawLine(new dvec3(realMin.x, realMin.y, realMin.z), new dvec3(realMin.x, realMin.y, realMax.z));
        DrawLine(new dvec3(realMin.x, realMin.y, realMax.z), new dvec3(realMax.x, realMin.y, realMax.z));
        DrawLine(new dvec3(realMax.x, realMin.y, realMax.z), new dvec3(realMax.x, realMin.y, realMin.z));
        DrawLine(new dvec3(realMax.x, realMin.y, realMin.z), new dvec3(realMin.x, realMin.y, realMin.z));


        DrawLine(new dvec3(realMin.x, realMax.y, realMin.z), new dvec3(realMin.x, realMax.y, realMax.z));
        DrawLine(new dvec3(realMin.x, realMax.y, realMax.z), new dvec3(realMax.x, realMax.y, realMax.z));
        DrawLine(new dvec3(realMax.x, realMax.y, realMax.z), new dvec3(realMax.x, realMax.y, realMin.z));
        DrawLine(new dvec3(realMax.x, realMax.y, realMin.z), new dvec3(realMin.x, realMax.y, realMin.z));


        DrawLine(new dvec3(realMin.x, realMin.y, realMin.z), new dvec3(realMin.x, realMax.y, realMin.z));
        DrawLine(new dvec3(realMin.x, realMin.y, realMax.z), new dvec3(realMin.x, realMax.y, realMax.z));
        DrawLine(new dvec3(realMax.x, realMin.y, realMax.z), new dvec3(realMax.x, realMax.y, realMax.z));
        DrawLine(new dvec3(realMax.x, realMin.y, realMin.z), new dvec3(realMax.x, realMax.y, realMin.z));
    }

    public static void DrawBox(Box box)
        => DrawBox(box, 0);

    public static void DrawBox(Box box, float expansion)
        => DrawCube(box.min, box.max, expansion);

    public override Pipeline CreatePipeline(PackManager packs, MainFramebuffer framebuffer) {
        if (!Client.renderSystem!.ShaderManager.GetShaders(new("shaders/debug"), out var shaders))
            throw new("Shaders not present.");

        return ResourceFactory.CreateGraphicsPipeline(new() {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new() {
                DepthComparison = ComparisonKind.LessEqual,
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
            },
            Outputs = framebuffer.Framebuffer.OutputDescription,
            PrimitiveTopology = PrimitiveTopology.LineList,
            RasterizerState = new() {
                CullMode = FaceCullMode.None,
                DepthClipEnabled = false,
                FillMode = PolygonFillMode.Wireframe,
                ScissorTestEnabled = false,
            },
            ResourceLayouts = [
                Client.gameRenderer!.CameraStateManager.CameraResourceLayout,
            ],
            ShaderSet = new() {
                VertexLayouts = [
                    DebugVertex.Layout
                ],
                Shaders = shaders
            }
        });
    }

    public override void Render(double delta)
        => Flush();

    public override void Dispose() {}

    private void Flush() {
        if (Vertices.Count == 0)
            return;

        VertexBuffer.UpdateImmediate(Vertices);
        VertexBuffer.Bind(0);
        
        RenderSystem.Draw(VertexBuffer.size);

        Vertices.Clear();
    }

    private void AddPoint(dvec3 pos) {
        Vertices.Vertex(new() {
            color = color,
            position = (matrix * new vec4((vec3)(pos - Client.gameRenderer!.MainCamera.position), 1)).xyz
        });
    }
}
