using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.Texture;
using Voxel.Common.Util;

namespace Voxel.Client.Rendering.VertexTypes;

public struct BasicVertex {

    public static readonly VertexLayoutDescription Layout = new(
        new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
        new VertexElementDescription("Color", VertexElementFormat.Float4, VertexElementSemantic.Color),
        new VertexElementDescription("UV", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("AO", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("UVMin", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("UVMax", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );

    public vec3 position;
    public vec4 color;
    public vec2 uv;
    public vec2 ao;
    public vec2 uvmin;
    public vec2 uvmax;

    public BasicVertex() {}

    public BasicVertex(vec3 pos) : this(pos, vec4.Ones, vec2.Zero, vec2.Zero) {}

    public BasicVertex(vec3 pos, vec4 color) : this(pos, color, vec2.Zero, vec2.Zero) {}

    public BasicVertex(vec3 pos, vec4 color, vec2 uv) : this(pos, color, uv, vec2.Zero) {}

    public BasicVertex(vec3 pos, vec4 color, vec2 uv, vec2 ao) : this(pos, color, uv, ao, vec2.Zero, vec2.Ones) {}
    
    public BasicVertex(vec3 pos, vec4 color, vec2 uv, vec2 ao, Atlas.Sprite sprite) : this(pos, color, uv, ao, sprite.uvPosition, sprite.uvPosition + sprite.uvSize) {}
    
    public BasicVertex(vec3 pos, vec4 color, vec2 uv, vec2 ao, vec2 uvmin, vec2 uvmax) {
        position = pos;
        this.color = color;
        this.uv = uv;
        this.ao = ao;
        this.uvmin = uvmin;
        this.uvmax = uvmax;
    }

    public static implicit operator Packed(BasicVertex vertex) => new() {
        Position = vertex.position,
        Color = vertex.color.Packed(),
        UV = ((int)(vertex.uv.x * ushort.MaxValue)) | ((int)(vertex.uv.y * ushort.MaxValue)) << 16,
        UVMin = vertex.uvmin,
        UVMax = vertex.uvmax
    };

    public struct Packed {
        public static readonly VertexLayoutDescription Layout = new(
            new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
            new VertexElementDescription("Color", VertexElementFormat.Int1, VertexElementSemantic.Color),
            new VertexElementDescription("UV", VertexElementFormat.Int1, VertexElementSemantic.TextureCoordinate),
            new VertexElementDescription("AO", VertexElementFormat.Float1, VertexElementSemantic.TextureCoordinate),
            new VertexElementDescription("UVMin", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
            new VertexElementDescription("UVMax", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
        );

        public vec3 Position;
        public int Color;
        public int UV;
        public float AO;
        public vec2 UVMin;
        public vec2 UVMax;
    }
}
