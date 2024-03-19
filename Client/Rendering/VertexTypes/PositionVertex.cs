using GlmSharp;
using Veldrid;

namespace Voxel.Client.Rendering.VertexTypes;

public struct PositionVertex {
    public static readonly VertexLayoutDescription Layout = new(
        new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position)
    );

    public vec3 position;

    public PositionVertex(vec3 position) {
        this.position = position;
    }
}
