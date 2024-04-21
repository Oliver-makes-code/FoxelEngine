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

    public readonly Position2dVertex WithAoCoord(vec2 aoCoord)
        => this;

    public readonly Position2dVertex WithColor(vec4 color)
        => this;

    public readonly Position2dVertex WithPosition(vec3 position)
        => new() {
            position = position.xy
        };

    public readonly Position2dVertex WithUv(vec2 uv)
        => this;
    
    public readonly Position2dVertex WithUvMax(vec2 uvMax)
        => this;

    public readonly Position2dVertex WithUvMin(vec2 uvMin)
        => this;
}
