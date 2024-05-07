using GlmSharp;
using Veldrid;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.VertexTypes;

public struct PositionVertex : Vertex<PositionVertex> {
    public static VertexLayoutDescription Layout { get; } = new(
        new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position)
    );

    public vec3 position;

    public PositionVertex(vec3 position) {
        this.position = position;
    }

    public readonly PositionVertex WithPosition(vec3 position)
        => new() {
            position = position
        };
}
