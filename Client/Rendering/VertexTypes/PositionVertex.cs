using GlmSharp;
using Veldrid;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.VertexTypes;

public struct PositionVertex : Vertex<PositionVertex> {
    public static readonly VertexLayoutDescription Layout = new(
        new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position)
    );

    public vec3 position;

    public PositionVertex(vec3 position) {
        this.position = position;
    }

    public readonly PositionVertex WithAoCoord(vec2 aoCoord)
        => this;

    public readonly PositionVertex WithColor(vec4 color)
        => this;

    public readonly PositionVertex WithPosition(vec3 position)
        => new() {
            position = position
        };

    public readonly PositionVertex WithUv(vec2 uv)
        => this;
    
    public readonly PositionVertex WithUvMax(vec2 uvMax)
        => this;

    public readonly PositionVertex WithUvMin(vec2 uvMin)
        => this;
}
