using GlmSharp;
using Veldrid;

namespace Foxel.Client.Rendering.VertexTypes.Entities;

public struct EntityMaterialVertex {
    public static VertexLayoutDescription Layout { get; } = new(
        new VertexElementDescription("UvMin", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("UvMax", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );

    public vec2 uvMin;
    public vec2 uvMax;
}
