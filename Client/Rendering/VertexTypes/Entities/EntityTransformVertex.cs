using Foxel.Core.Rendering;
using GlmSharp;
using Veldrid;

namespace Foxel.Client.Rendering.VertexTypes.Entities;

public struct EntityTransformVertex : Vertex<EntityTransformVertex> {
    public static VertexLayoutDescription Layout { get; } = new(
        new VertexElementDescription("MatrixA", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("MatrixB", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("MatrixC", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("MatrixD", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate)
    ) {
        InstanceStepRate = 24
    };

    public mat4 matrix;
}
