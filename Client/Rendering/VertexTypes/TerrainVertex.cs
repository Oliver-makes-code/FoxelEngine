using GlmSharp;
using Veldrid;
using Foxel.Client.Rendering.Texture;
using Foxel.Client.Rendering.Utils;
using Foxel.Common.Util;
using Foxel.Core.Rendering;

namespace Foxel.Client.Rendering.VertexTypes;

public struct TerrainVertex : Vertex<TerrainVertex> {
    public static VertexLayoutDescription Layout { get; } = new(
        new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
        new VertexElementDescription("Color", VertexElementFormat.Float3, VertexElementSemantic.Color),
        new VertexElementDescription("Uv", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("UvMin", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("UvMax", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("Normal", VertexElementFormat.Float3, VertexElementSemantic.Normal)
    );

    public vec3 position;
    public vec3 color;
    public vec2 uv;
    public vec2 uvMin;
    public vec2 uvMax;
    public vec3 normal;
    
    public TerrainVertex(vec3 pos, vec3 color, vec2 uv, Atlas.Sprite sprite, vec3 normal) : this(pos, color, uv, sprite.uvPosition, sprite.uvPosition + sprite.uvSize, normal) {}
    
    public TerrainVertex(vec3 pos, vec3 color, vec2 uv, vec2 uvMin, vec2 uvMax, vec3 normal) {
        position = pos;
        this.color = color;
        this.uv = uv;
        this.uvMin = uvMin;
        this.uvMax = uvMax;
        this.normal = normal;
    }

    public static Packed Pack(TerrainVertex vertex) 
        => new() {
            position = vertex.position,
            color = PackColorAndAo(vertex.color),
            uv = PackUv(vertex.uv),
            uvMin = PackUv(vertex.uvMin),
            uvMax = PackUv(vertex.uvMax),
            normal = vertex.normal
        };
    
    public static int PackColorAndAo(vec3 color)
        => new vec4(color, 0).Packed();

    private static int PackUv(vec2 uv)
        => ((int)(uv.x * ushort.MaxValue)) | ((int)(uv.y * ushort.MaxValue)) << 16;

    public readonly TerrainVertex WithAoCoord(vec2 ao)
        => new() {
            position = position,
            color = color,
            uv = uv,
            uvMin = uvMin,
            uvMax = uvMax,
            normal = normal
        };

    public readonly TerrainVertex WithColor(vec4 color)
        => new() {
            position = position,
            color = color.rgb,
            uv = uv,
            uvMin = uvMin,
            uvMax = uvMax,
            normal = normal
        };

    public readonly TerrainVertex WithPosition(vec3 position)
        => new() {
            position = position,
            color = color,
            uv = uv,
            uvMin = uvMin,
            uvMax = uvMax,
            normal = normal
        };

    public readonly TerrainVertex WithUv(vec2 uv)
        => new() {
            position = position,
            color = color,
            uv = uv,
            uvMin = uvMin,
            uvMax = uvMax,
            normal = normal
        };
    
    public readonly TerrainVertex WithUvMax(vec2 uvMax)
        => new() {
            position = position,
            color = color,
            uv = uv,
            uvMin = uvMin,
            uvMax = uvMax,
            normal = normal
        };

    public readonly TerrainVertex WithUvMin(vec2 uvMin)
        => new() {
            position = position,
            color = color,
            uv = uv,
            uvMin = uvMin,
            uvMax = uvMax,
            normal = normal
        };

    public struct Packed : Vertex<Packed> {
        public static VertexLayoutDescription Layout { get; } = new(
            new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
            new VertexElementDescription("Color", VertexElementFormat.Int1, VertexElementSemantic.Color),
            new VertexElementDescription("Uv", VertexElementFormat.Int1, VertexElementSemantic.TextureCoordinate),
            new VertexElementDescription("UvMin", VertexElementFormat.Int1, VertexElementSemantic.TextureCoordinate),
            new VertexElementDescription("UvMax", VertexElementFormat.Int1, VertexElementSemantic.TextureCoordinate),
        new VertexElementDescription("Normal", VertexElementFormat.Float3, VertexElementSemantic.Normal)
        );

        // Could we pack this into a long? Could save us 4 bytes, but would only allow 32 positions per block (assuming 10 bits per axis)
        public vec3 position;
        public int color;
        // Could be packed to be two bools referencing either uvMin or uvMax, might not be worth it due to padding.
        // Could also be packed into a value between uvMin and uvMax
        public int uv;
        public int uvMin;
        public int uvMax;
        public vec3 normal;
    }
}
