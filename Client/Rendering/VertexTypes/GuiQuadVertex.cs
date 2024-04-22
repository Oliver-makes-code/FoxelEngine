using GlmSharp;
using Veldrid;
using Voxel.Common.Util;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.VertexTypes;

public struct GuiQuadVertex : Vertex<GuiQuadVertex> {
    public static readonly VertexLayoutDescription Layout = new(
        new VertexElementDescription("ScreenAnchor", VertexElementFormat.Float2, VertexElementSemantic.Position),
        new VertexElementDescription("TextureAnchor", VertexElementFormat.Float2, VertexElementSemantic.Position),
        new VertexElementDescription("Position", VertexElementFormat.Int2, VertexElementSemantic.Position),
        new VertexElementDescription("Size", VertexElementFormat.Int2, VertexElementSemantic.Position),
        new VertexElementDescription("Color", VertexElementFormat.Float4, VertexElementSemantic.Color),
        new VertexElementDescription("UvMin", VertexElementFormat.Float2, VertexElementSemantic.Color),
        new VertexElementDescription("UvMax", VertexElementFormat.Float2, VertexElementSemantic.Color)
    ) {
        InstanceStepRate = 1
    };

    public vec2 screenAnchor;
    public vec2 textureAnchor;
    public ivec2 position;
    public ivec2 size;
    public vec4 color;
    public vec2 uvMin;
    public vec2 uvMax;

    public readonly GuiQuadVertex WithScreenAnchor(vec2 screenAnchor)
        => new() {
            screenAnchor = screenAnchor,
            textureAnchor = textureAnchor,
            position = position,
            size = size,
            color = color,
            uvMax = uvMax,
            uvMin = uvMin
        };

    public readonly GuiQuadVertex WithTextureAnchor(vec2 textureAnchor)
        => new() {
            screenAnchor = screenAnchor,
            textureAnchor = textureAnchor,
            position = position,
            size = size,
            color = color,
            uvMax = uvMax,
            uvMin = uvMin
        };

    public readonly GuiQuadVertex WithPosition(ivec2 position)
        => new() {
            screenAnchor = screenAnchor,
            textureAnchor = textureAnchor,
            position = position,
            size = size,
            color = color,
            uvMax = uvMax,
            uvMin = uvMin
        };

    public readonly GuiQuadVertex WithSize(ivec2 size)
        => new() {
            screenAnchor = screenAnchor,
            textureAnchor = textureAnchor,
            position = position,
            size = size,
            color = color,
            uvMax = uvMax,
            uvMin = uvMin
        };

    public readonly GuiQuadVertex WithColor(vec4 color)
        => new() {
            screenAnchor = screenAnchor,
            textureAnchor = textureAnchor,
            position = position,
            size = size,
            color = color,
            uvMax = uvMax,
            uvMin = uvMin
        };

    public readonly GuiQuadVertex WithUvMax(vec2 uvMax)
        => new() {
            screenAnchor = screenAnchor,
            textureAnchor = textureAnchor,
            position = position,
            size = size,
            color = color,
            uvMax = uvMax,
            uvMin = uvMin
        };

    public readonly GuiQuadVertex WithUvMin(vec2 uvMin)
        => new() {
            screenAnchor = screenAnchor,
            textureAnchor = textureAnchor,
            position = position,
            size = size,
            color = color,
            uvMax = uvMax,
            uvMin = uvMin
        };

    public override string ToString()
        => $"GuiQuadVertex(({screenAnchor}), ({textureAnchor}), ({position}), ({size}), ({color}), ({uvMin}), ({uvMax}))";
}
