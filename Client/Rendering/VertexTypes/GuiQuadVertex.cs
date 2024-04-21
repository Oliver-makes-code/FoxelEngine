using GlmSharp;
using Veldrid;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.VertexTypes;

public struct GuiQuadVertex : Vertex<GuiQuadVertex> {
    public static readonly VertexLayoutDescription Layout = new(
        new VertexElementDescription("Anchor", VertexElementFormat.Float2, VertexElementSemantic.Position),
        new VertexElementDescription("Position", VertexElementFormat.Int2, VertexElementSemantic.Position),
        new VertexElementDescription("Size", VertexElementFormat.Int2, VertexElementSemantic.Position),
        new VertexElementDescription("Color", VertexElementFormat.Float4, VertexElementSemantic.Color)
    ) {
        InstanceStepRate = 1
    };

    public vec2 anchor;
    public ivec2 position;
    public ivec2 size;
    public vec4 color;

    public GuiQuadVertex WithAoCoord(vec2 ao)
        => this;

    public GuiQuadVertex WithColor(vec4 color)
        => this;

    public GuiQuadVertex WithPosition(vec3 position)
        => this;

    public GuiQuadVertex WithUv(vec2 uv)
        => this;

    public GuiQuadVertex WithUvMax(vec2 uvMax)
        => this;

    public GuiQuadVertex WithUvMin(vec2 uvMin)
        => this;
}