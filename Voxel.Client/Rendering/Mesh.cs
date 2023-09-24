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
    public readonly uint[] indices;

    public Mesh(MeshBuilder builder) {
        var quads = builder.quads.ToArray();
        vertices = new VertexPositionColorTexture[quads.Length * 4];
        indices = new uint[quads.Length * 6];
        for (int i = 0; i < quads.Length; i++) {
            var quad = quads[i];
            vertices[i*4+0] = quad.a;
            vertices[i*4+1] = quad.b;
            vertices[i*4+2] = quad.c;
            vertices[i*4+3] = quad.d;
            for (uint j = 0; j < 6; j++) {
                indices[i*6+j] = (uint)(Quad.indices[j]+i*4);
            }
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

    public static readonly uint[] indices = { 0, 1, 2, 0, 2, 3 };
}
