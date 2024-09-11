using GlmSharp;
using Veldrid;

namespace Foxel.Client.Rendering.VertexTypes.Entities;

public struct EntityBaseVertex {
    public static VertexLayoutDescription Layout { get; } = new(
        new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
        new VertexElementDescription("Normal", VertexElementFormat.Float3, VertexElementSemantic.Color),
        new VertexElementDescription("Uv", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );

    public vec3 position;
    public vec3 normal;
    public vec2 uv;
}
