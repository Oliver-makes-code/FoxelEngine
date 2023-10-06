using GlmSharp;
using Veldrid;

namespace Voxel.Client.Rendering.VertexTypes;

public struct BasicVertex {

    public static readonly VertexLayoutDescription Layout = new(
        new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
        new VertexElementDescription("Color", VertexElementFormat.Float4, VertexElementSemantic.Color),
        new VertexElementDescription("UV", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );

    public vec3 position;
    public vec4 color;
    public vec2 uv;

    public BasicVertex() {}

    public BasicVertex(vec3 pos) : this(pos, vec4.Ones, vec2.Zero) {}

    public BasicVertex(vec3 pos, vec4 color) : this(pos, color, vec2.Zero) {}
    
    public BasicVertex(vec3 pos, vec4 color, vec2 uv) {
        position = pos;
        this.color = color;
        this.uv = uv;
    }

    public static implicit operator Packed(BasicVertex vertex)
        => new();

    //TODO - Implement
    public struct Packed {

    }
}
