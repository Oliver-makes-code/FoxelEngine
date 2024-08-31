using GlmSharp;
using Veldrid;
using Foxel.Core.Rendering;

namespace Foxel.Client.Rendering.VertexTypes;

public struct DebugVertex : Vertex<DebugVertex> {
    public static VertexLayoutDescription Layout { get; } = new(
        new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
        new VertexElementDescription("Color", VertexElementFormat.Float4, VertexElementSemantic.Color),
        new VertexElementDescription("UV", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );

    public vec3 position;
    public vec4 color;
    public vec2 uv;

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
}
