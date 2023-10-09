using GlmSharp;
using Veldrid;
using Voxel.Common.Util;

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

    public static implicit operator Packed(BasicVertex vertex) => new Packed {
        Position = vertex.position,
        Color = vertex.color.Packed(),
        UV = ((int)(vertex.uv.x * ushort.MaxValue)) | ((int)(vertex.uv.y * ushort.MaxValue)) << 16
    };

    //TODO - Implement
    public struct Packed {
        public static readonly VertexLayoutDescription Layout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
            new VertexElementDescription("Color", VertexElementFormat.Int1, VertexElementSemantic.Color),
            new VertexElementDescription("UV", VertexElementFormat.Int1, VertexElementSemantic.TextureCoordinate)
        );

        public vec3 Position;
        public int Color;
        public int UV;
    }
}
