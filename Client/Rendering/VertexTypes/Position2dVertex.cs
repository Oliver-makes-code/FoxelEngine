using GlmSharp;
using Veldrid;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.VertexTypes;

public struct Position2dVertex : Vertex<Position2dVertex> {
    public static readonly VertexLayoutDescription Layout = new(
        new VertexElementDescription("Position", VertexElementFormat.Float2, VertexElementSemantic.Position)
    );

    public vec2 position;

    public Position2dVertex(vec2 position) {
        this.position = position;
    }

    public readonly Position2dVertex WithPosition(vec2 position)
        => new() {
            position = position
        };
}
