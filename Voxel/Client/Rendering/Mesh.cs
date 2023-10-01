using Microsoft.Xna.Framework.Graphics;

namespace Voxel.Client.Rendering;

public class MeshBuilder {
    public static Quad[,] Quads;
    public int threadNumber;
    public int idx;

    public static void SetupThreadCount(int threadCount) {
        Quads = new Quad[threadCount,32*32*32*6];
    }

    public MeshBuilder(int threadNumber = 0) {
        this.threadNumber = threadNumber;
        idx = 0;
    }
    
    public void Quad(
        VertexPositionColorTexture a,
        VertexPositionColorTexture b,
        VertexPositionColorTexture c,
        VertexPositionColorTexture d
    ) {
        Quads[threadNumber, idx++] = new(a, b, c, d);
    }
    
    public void Quad(VertexPositionColorTexture[] vertices) {
        Quads[threadNumber, idx++] = new(vertices[0], vertices[1], vertices[2], vertices[3]);
    }

    public Mesh Build() => new(this);
}

public readonly struct Mesh {
    public static VertexPositionColorTexture[][] vertices;

    public static void SetupThreadCount(int threadCount) {
        vertices = new VertexPositionColorTexture[threadCount][];
        for (int i = 0; i < threadCount; i++) {
            vertices[i] = new VertexPositionColorTexture[32*32*32*6*4];
        }
    }

    public Mesh(MeshBuilder builder) {
        var thread = builder.threadNumber;
        for (int i = 0; i < builder.idx; i++) {
            var quad = MeshBuilder.Quads[thread, i];
            vertices[thread][i*4+0] = quad.a;
            vertices[thread][i*4+1] = quad.b;
            vertices[thread][i*4+2] = quad.c;
            vertices[thread][i*4+3] = quad.d;
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

    public const int IndexBufferQuads = 32*32*32*6;
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
