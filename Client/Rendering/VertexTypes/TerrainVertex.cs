using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.Texture;
using Voxel.Client.Rendering.Utils;
using Voxel.Common.Util;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.VertexTypes;

public struct TerrainVertex : Vertex<TerrainVertex> {
    public vec3 position;
    public vec3 color;
    public vec2 uv;
    public vec2 ao;
    public vec2 uvMin;
    public vec2 uvMax;
    
    public TerrainVertex(vec3 pos, vec3 color, vec2 uv, vec2 ao, Atlas.Sprite sprite) : this(pos, color, uv, ao, sprite.uvPosition, sprite.uvPosition + sprite.uvSize) {}
    
    public TerrainVertex(vec3 pos, vec3 color, vec2 uv, vec2 ao, vec2 uvMin, vec2 uvMax) {
        position = pos;
        this.color = color;
        this.uv = uv;
        this.ao = ao;
        this.uvMin = uvMin;
        this.uvMax = uvMax;
    }

    public static Packed Pack(TerrainVertex vertex, vec4 ao) 
        => new() {
            position = vertex.position,
            colorAndAo = PackColorAndAo(vertex.color, RenderingUtils.BiliniearInterpolation(ao, vertex.ao)),
            uv = PackUv(vertex.uv),
            uvMin = PackUv(vertex.uvMin),
            uvMax = PackUv(vertex.uvMax)
        };
    
    public static int PackColorAndAo(vec3 color, float ao)
        => new vec4(color, ao / 3).Packed();

    private static int PackUv(vec2 uv)
        => ((int)(uv.x * ushort.MaxValue)) | ((int)(uv.y * ushort.MaxValue)) << 16;

    public readonly TerrainVertex WithAoCoord(vec2 ao)
        => new() {
            position = position,
            color = color,
            uv = uv,
            ao = ao,
            uvMin = uvMin,
            uvMax = uvMax
        };

    public readonly TerrainVertex WithColor(vec4 color)
        => new() {
            position = position,
            color = color.rgb,
            uv = uv,
            ao = ao,
            uvMin = uvMin,
            uvMax = uvMax
        };

    public readonly TerrainVertex WithPosition(vec3 position)
        => new() {
            position = position,
            color = color,
            uv = uv,
            ao = ao,
            uvMin = uvMin,
            uvMax = uvMax
        };

    public readonly TerrainVertex WithUv(vec2 uv)
        => new() {
            position = position,
            color = color,
            uv = uv,
            ao = ao,
            uvMin = uvMin,
            uvMax = uvMax
        };
    
    public readonly TerrainVertex WithUvMax(vec2 uvMax)
        => new() {
            position = position,
            color = color,
            uv = uv,
            ao = ao,
            uvMin = uvMin,
            uvMax = uvMax
        };

    public readonly TerrainVertex WithUvMin(vec2 uvMin)
        => new() {
            position = position,
            color = color,
            uv = uv,
            ao = ao,
            uvMin = uvMin,
            uvMax = uvMax
        };

    public struct Packed {
        public static readonly VertexLayoutDescription Layout = new(
            new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.Position),
            new VertexElementDescription("ColorAndAo", VertexElementFormat.Int1, VertexElementSemantic.Color),
            new VertexElementDescription("Uv", VertexElementFormat.Int1, VertexElementSemantic.TextureCoordinate),
            new VertexElementDescription("UvMin", VertexElementFormat.Int1, VertexElementSemantic.TextureCoordinate),
            new VertexElementDescription("UvMax", VertexElementFormat.Int1, VertexElementSemantic.TextureCoordinate)
        );

        // Could we pack this into a long? Could save us 4 bytes, but would only allow 32 positions per block (assuming 10 bits per axis)
        public vec3 position;
        public int colorAndAo;
        // Could be packed to be two bools referencing either uvMin or uvMax, might not be worth it due to padding.
        // Could also be packed into a value between uvMin and uvMax
        public int uv;
        public int uvMin;
        public int uvMax;
    }
}
