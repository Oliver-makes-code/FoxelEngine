using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Voxel.Client.Rendering;

public class MeshBuilder {
    public List<Quad> quads = new();

    public void Quad(
        VertexPositionColorTexture a,
        VertexPositionColorTexture b,
        VertexPositionColorTexture c,
        VertexPositionColorTexture d
    ) {
        quads.Add(new(a, b, c, d));
    }

    public Mesh Build() => new(this);
}

public readonly struct Mesh {
    public readonly VertexPositionColorTexture[] vertices;

    public Mesh(MeshBuilder builder) {
        var quads = builder.quads.ToArray();
        vertices = new VertexPositionColorTexture[quads.Length * 4];
        for (int i = 0; i < quads.Length; i++) {
            var quad = quads[i];
            vertices[i*4+0] = quad.a;
            vertices[i*4+1] = quad.b;
            vertices[i*4+2] = quad.c;
            vertices[i*4+3] = quad.d;
        }
    }
}

public struct Quad {
    public VertexPositionColorTexture a;
    public VertexPositionColorTexture b;
    public VertexPositionColorTexture c;
    public VertexPositionColorTexture d;

    public Quad(
        VertexPositionColorTexture a,
        VertexPositionColorTexture b,
        VertexPositionColorTexture c,
        VertexPositionColorTexture d
    ) {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
    }

    public const int IndexBufferQuads = 32*32*32;
    public const int IndexBufferStride = 6;
    public const int IndexBufferCount = IndexBufferQuads*IndexBufferStride;
    public static readonly uint[] indices = { 0, 1, 2, 0, 2, 3 };
    public static uint[] GenerateCommonIndexBufferArray() {
        var output = new uint[IndexBufferCount];

        for (var i = 0; i < IndexBufferQuads; i++) {
            for (var j = 0; j < IndexBufferStride; j++) {
                output[i*IndexBufferStride+j] = (uint)(indices[j] + i*4);
            }
        }

        return output;
    }

    public static IndexBuffer GenerateCommonIndexBuffer(GraphicsDevice device) {
        var arr = GenerateCommonIndexBufferArray();
        var buffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, IndexBufferCount, BufferUsage.WriteOnly);
        buffer.SetData(arr);
        return buffer;
    }
}
