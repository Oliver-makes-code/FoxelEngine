using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Voxel.Client.Rendering;

public class MeshBuilder {
    public List<Quad> quads = new();

    public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color) {
        quads.Add(new(a, b, c, d, color));
    }

    public Mesh Build() => new(this);
}

public readonly struct Mesh {
    public readonly VertexPositionColor[] vertices;
    public readonly uint[] indices;

    public Mesh(MeshBuilder builder) {
        var quads = builder.quads.ToArray();
        vertices = new VertexPositionColor[quads.Length * 4];
        indices = new uint[quads.Length * 6];
        for (int i = 0; i < quads.Length; i++) {
            var quad = quads[i];
            vertices[i*4+0] = new(quad.a, quad.color);
            vertices[i*4+1] = new(quad.b, quad.color);
            vertices[i*4+2] = new(quad.c, quad.color);
            vertices[i*4+3] = new(quad.d, quad.color);
            for (uint j = 0; j < 6; j++) {
                indices[i*6+j] = (uint)(Quad.indices[j]+i*4);
            }
        }
    }
}

public struct Quad {
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;
    public Vector3 d;
    public Color color;

    public Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color) {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
        this.color = color;
    }

    public static readonly uint[] indices = { 0, 1, 2, 0, 2, 3 };
}
