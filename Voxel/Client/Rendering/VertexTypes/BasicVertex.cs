using GlmSharp;

namespace Voxel.Client.Rendering.VertexTypes;

public struct BasicVertex {
    public vec3 Position;
    public vec4 Color;
    public vec2 UV;

    public BasicVertex() {

    }

    public BasicVertex(vec3 pos) : this(pos, vec4.Ones, vec2.Zero) {

    }

    public BasicVertex(vec3 pos, vec4 color) : this(pos, color, vec2.Zero) {

    }
    public BasicVertex(vec3 pos, vec4 color, vec2 uv) {
        Position = pos;
        Color = color;
        UV = uv;
    }

    public static implicit operator Packed(BasicVertex vertex) => new Packed();

    //TODO - Implement
    public struct Packed {

    }
}
