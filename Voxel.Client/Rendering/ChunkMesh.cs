using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxel.Common.World;

namespace Voxel.Client.Rendering;

public class ChunkMesh {
    public int primitiveCount;
    public readonly VertexBuffer vertices;
    readonly IndexBuffer indices;

    public ChunkMesh(GraphicsDevice device, Chunk chunk, Vector3 offset) {
        var mesh = BuildChunk(chunk, offset);
        vertices = new(device, typeof(VertexPositionColor), mesh.vertices.Length, BufferUsage.WriteOnly);
        indices = new(device, IndexElementSize.ThirtyTwoBits, mesh.indices.Length, BufferUsage.WriteOnly);
        vertices.SetData(mesh.vertices);
        indices.SetData(mesh.indices);

        primitiveCount = mesh.indices.Length / 3;
    }

    public void Draw(GraphicsDevice device, Effect effect) {
        device.SetVertexBuffer(vertices);
        device.Indices = indices;

        effect.CurrentTechnique.Passes[0].Apply();

        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
    }

    public static Mesh BuildRandomChunk() => BuildChunk(new(), new(0, 0, 0));

    public static Mesh BuildChunk(Chunk chunk, Vector3 offset) {
        MeshBuilder builder = new();
        BuildChunk(chunk, builder, offset);
        return builder.Build();
    }

    public static void BuildChunk(Chunk chunk, MeshBuilder builder, Vector3 offset) {
        for (byte x = 0; x < 0b10_0000u; x++) {
            for (byte y = 0; y < 0b10_0000u; y++) {
                for (byte z = 0; z < 0b10_0000u; z++) {
                    GenerateCube(builder, chunk, x, y, z, offset);
                }
            }
        }
    }

    public static void GenerateCube(MeshBuilder builder, Chunk chunk, byte x, byte y, byte z, Vector3 offset) {
        var block = chunk[false, x, y, z];
        if (block == 0)
            return;

        if (chunk[false, x, y, (byte)(z-1)] == 0) {
            builder.Quad(
                new Vector3(x, y, z) + offset,
                new Vector3(x+1, y, z) + offset,
                new Vector3(x+1, y+1, z) + offset,
                new Vector3(x, y+1, z) + offset,
                Color.Red
            );
        }

        if (chunk[false, x, y, (byte)(z+1)] == 0) {
            builder.Quad(
                new Vector3(x, y, z+1) + offset,
                new Vector3(x, y+1, z+1) + offset,
                new Vector3(x+1, y+1, z+1) + offset,
                new Vector3(x+1, y, z+1) + offset,
                Color.Orange
            );
        }

        if (chunk[false, x, (byte)(y+1), z] == 0) {
            builder.Quad(
                new Vector3(x, y+1, z) + offset,
                new Vector3(x+1, y+1, z) + offset,
                new Vector3(x+1, y+1, z+1) + offset,
                new Vector3(x, y+1, z+1) + offset,
                Color.Yellow
            );
        }

        if (chunk[false, x, (byte)(y-1), z] == 0) {
            builder.Quad(
                new Vector3(x, y, z+1) + offset,
                new Vector3(x+1, y, z+1) + offset,
                new Vector3(x+1, y, z) + offset,
                new Vector3(x, y, z) + offset,
                Color.Green
            );
        }

        if (chunk[false, (byte)(x-1), y, z] == 0) {
            builder.Quad(
                new Vector3(x, y, z) + offset,
                new Vector3(x, y+1, z) + offset,
                new Vector3(x, y+1, z+1) + offset,
                new Vector3(x, y, z+1) + offset,
                Color.Blue
            );
        }


        if (chunk[false, (byte)(x+1), y, z] == 0) {
            builder.Quad(
                new Vector3(x+1, y, z+1) + offset,
                new Vector3(x+1, y+1, z+1) + offset,
                new Vector3(x+1, y+1, z) + offset,
                new Vector3(x+1, y, z) + offset,
                Color.Purple
            );
        }
    }
}