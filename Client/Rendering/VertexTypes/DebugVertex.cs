using GlmSharp;
using Veldrid;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.VertexTypes;

public struct DebugVertex : Vertex<DebugVertex> {
    public static readonly VertexLayoutDescription Layout = new(
        new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
        new VertexElementDescription("Color", VertexElementFormat.Float4, VertexElementSemantic.Color),
        new VertexElementDescription("UV", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );

    public vec3 position;
    public vec4 color;
    public vec2 uv;

    public readonly DebugVertex WithAoCoord(vec2 aoCoord)
        => this;

    public readonly DebugVertex WithColor(vec4 color)
        => new() {
            position = position,
            color = color,
            uv = uv
        };

    public readonly DebugVertex WithPosition(vec3 position)
        => new() {
            position = position,
            color = color,
            uv = uv
        };

    public readonly DebugVertex WithUv(vec2 uv)
        => new() {
            position = position,
            color = color,
            uv = uv
        };
    
    public readonly DebugVertex WithUvMax(vec2 uvMax)
        => this;

    public readonly DebugVertex WithUvMin(vec2 uvMin)
        => this;
}
