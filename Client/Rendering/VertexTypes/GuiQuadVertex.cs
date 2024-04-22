using GlmSharp;
using Veldrid;
using Voxel.Common.Util;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.VertexTypes;

public struct GuiQuadVertex : Vertex<GuiQuadVertex> {
    public static readonly VertexLayoutDescription Layout = new(
        new VertexElementDescription("Anchor", VertexElementFormat.Float2, VertexElementSemantic.Position),
        new VertexElementDescription("Position", VertexElementFormat.Int2, VertexElementSemantic.Position),
        new VertexElementDescription("Size", VertexElementFormat.Int2, VertexElementSemantic.Position),
        new VertexElementDescription("Color", VertexElementFormat.Float4, VertexElementSemantic.Color),
        new VertexElementDescription("UvMin", VertexElementFormat.Float2, VertexElementSemantic.Color),
        new VertexElementDescription("UvMax", VertexElementFormat.Float2, VertexElementSemantic.Color)
    ) {
        InstanceStepRate = 1
    };

    public vec2 anchor;
    public ivec2 position;
    public ivec2 size;
    public vec4 color;
    public vec2 uvMin;
    public vec2 uvMax;

    public readonly GuiQuadVertex WithAnchor(vec2 anchor)
        => new() {
            anchor = anchor,
            position = position,
            size = size,
            color = color,
            uvMax = uvMax,
            uvMin = uvMin
        };

    public readonly GuiQuadVertex WithPosition(ivec2 position)
        => new() {
            anchor = anchor,
            position = position,
            size = size,
            color = color,
            uvMax = uvMax,
            uvMin = uvMin
        };

    public readonly GuiQuadVertex WithSize(ivec2 size)
        => new() {
            anchor = anchor,
            position = position,
            size = size,
            color = color,
            uvMax = uvMax,
            uvMin = uvMin
        };

    public readonly GuiQuadVertex WithColor(vec4 color)
        => new() {
            anchor = anchor,
            position = position,
            size = size,
            color = color,
            uvMax = uvMax,
            uvMin = uvMin
        };

    public readonly GuiQuadVertex WithUvMax(vec2 uvMax)
        => new() {
            anchor = anchor,
            position = position,
            size = size,
            color = color,
            uvMax = uvMax,
            uvMin = uvMin
        };

    public readonly GuiQuadVertex WithUvMin(vec2 uvMin)
        => new() {
            anchor = anchor,
            position = position,
            size = size,
            color = color,
            uvMax = uvMax,
            uvMin = uvMin
        };
}
