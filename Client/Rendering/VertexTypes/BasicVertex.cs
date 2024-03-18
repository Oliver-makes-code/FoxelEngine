using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.Texture;
using Voxel.Client.Rendering.Utils;
using Voxel.Common.Util;

namespace Voxel.Client.Rendering.VertexTypes;

public struct BasicVertex {

    public static readonly VertexLayoutDescription Layout = new(
        new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
        new VertexElementDescription("Color", VertexElementFormat.Float3, VertexElementSemantic.Color),
        new VertexElementDescription("UV", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("AO", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("UVMin", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("UVMax", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate)
    );

    public vec3 position;
    public vec3 color;
    public vec2 uv;
    public vec2 ao;
    public vec2 uvMin;
    public vec2 uvMax;

    public BasicVertex() {}

    public BasicVertex(vec3 pos) : this(pos, vec3.Ones, vec2.Zero, vec2.Zero) {}

    public BasicVertex(vec3 pos, vec3 color) : this(pos, color, vec2.Zero, vec2.Zero) {}

    public BasicVertex(vec3 pos, vec3 color, vec2 uv) : this(pos, color, uv, vec2.Zero) {}

    public BasicVertex(vec3 pos, vec3 color, vec2 uv, vec2 ao) : this(pos, color, uv, ao, vec2.Zero, vec2.Ones) {}
    
    public BasicVertex(vec3 pos, vec3 color, vec2 uv, vec2 ao, Atlas.Sprite sprite) : this(pos, color, uv, ao, sprite.uvPosition, sprite.uvPosition + sprite.uvSize) {}
    
    public BasicVertex(vec3 pos, vec3 color, vec2 uv, vec2 ao, vec2 uvMin, vec2 uvMax) {
        position = pos;
        this.color = color;
        this.uv = uv;
        this.ao = ao;
        this.uvMin = uvMin;
        this.uvMax = uvMax;
    }

    public static Packed Pack(BasicVertex vertex, vec4 ao) 
        => new() {
            position = vertex.position,
            color = PackColorAndAo(vertex.color, RenderingUtils.BiliniearInterpolation(ao, vertex.ao)),
            uv = PackUv(vertex.uv),
            uvMin = PackUv(vertex.uvMin),
            uvMax = PackUv(vertex.uvMax)
        };
    
    public static int PackColorAndAo(vec3 color, float ao)
        => new vec4(color, ao / 3).Packed();

    private static int PackUv(vec2 uv)
        => ((int)(uv.x * ushort.MaxValue)) | ((int)(uv.y * ushort.MaxValue)) << 16;

    public struct Packed {
        public static readonly VertexLayoutDescription Layout = new(
            new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
            new VertexElementDescription("Color", VertexElementFormat.Int1, VertexElementSemantic.Color),
            new VertexElementDescription("UV", VertexElementFormat.Int1, VertexElementSemantic.TextureCoordinate),
            new VertexElementDescription("UVMin", VertexElementFormat.Int1, VertexElementSemantic.TextureCoordinate),
            new VertexElementDescription("UVMax", VertexElementFormat.Int1, VertexElementSemantic.TextureCoordinate)
        );

        public vec3 position;
        public int color;
        public int uv;
        public int uvMin;
        public int uvMax;
    }
}
