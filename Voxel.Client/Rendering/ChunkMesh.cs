using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxel.Client.World;
using Voxel.Common.World;

namespace Voxel.Client.Rendering;

public class ChunkMesh {
    public int primitiveCount;
    public readonly VertexBuffer? vertices = null;
    readonly IndexBuffer? indices = null;

    public ChunkMesh(GraphicsDevice device, ClientWorld world, ChunkPos pos) {
        var mesh = BuildChunk(world, pos);
        if (mesh.vertices.Length != 0) {
            vertices = new(device, typeof(VertexPositionColor), mesh.vertices.Length, BufferUsage.WriteOnly);
            indices = new(device, IndexElementSize.ThirtyTwoBits, mesh.indices.Length, BufferUsage.WriteOnly);
            vertices.SetData(mesh.vertices);
            indices.SetData(mesh.indices);
        }

        primitiveCount = mesh.indices.Length / 3;
    }

    public void Draw(GraphicsDevice device, Effect effect) {
        if (vertices == null)
            return;

        device.SetVertexBuffer(vertices);
        device.Indices = indices;

        effect.CurrentTechnique.Passes[0].Apply();

        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
    }

    public static Mesh BuildChunk(ClientWorld world, ChunkPos pos) {
        MeshBuilder builder = new();
        BuildChunk(world, pos, builder);
        return builder.Build();
    }

    public static void BuildChunk(ClientWorld world, ChunkPos pos, MeshBuilder builder) {
        var chunk = world.world[pos] ?? new();
        var n = world.world[new(pos.x, pos.y, pos.z - 1)] ?? new();
        var s = world.world[new(pos.x, pos.y, pos.z + 1)] ?? new();
        var e = world.world[new(pos.x + 1, pos.y, pos.z)] ?? new();
        var w = world.world[new(pos.x - 1, pos.y, pos.z)] ?? new();
        for (byte x = 0; x < 0b10_0000u; x++) {
            for (byte y = 0; y < 0b10_0000u; y++) {
                for (byte z = 0; z < 0b10_0000u; z++) {
                    GenerateCube(builder, chunk, pos, x, y, z, n, s, e, w);
                }
            }
        }
    }

    public static void GenerateCube(MeshBuilder builder, Chunk chunk, ChunkPos pos, byte x, byte y, byte z, Chunk n, Chunk s, Chunk e, Chunk w) {
        var block = chunk[false, x, y, z];
        if (block == 0)
            return;

        var offset = pos.ToVector();

        var xd = x - 1;
        var xu = x + 1;
        var yd = y - 1;
        var yu = y + 1;
        var zd = z - 1;
        var zu = z + 1;

        if ((zd >= 0 ? chunk[false, x, y, (byte)zd] : n[false, x, y, 31]) == 0) {
            builder.Quad(
                new Vector3(x, y, z) + offset,
                new Vector3(xu, y, z) + offset,
                new Vector3(xu, yu, z) + offset,
                new Vector3(x, yu, z) + offset,
                Color.Red
            );
        }

        if ((zu <= 31 ? chunk[false, x, y, (byte)zu] : s[false, x, y, 0]) == 0) {
            builder.Quad(
                new Vector3(x, y, zu) + offset,
                new Vector3(x, yu, zu) + offset,
                new Vector3(xu, yu, zu) + offset,
                new Vector3(xu, y, zu) + offset,
                Color.Orange
            );
        }

        if ((yu <= 31 ? chunk[false, x, (byte)yu, z] : 0) == 0) {
            builder.Quad(
                new Vector3(x, yu, z) + offset,
                new Vector3(xu, yu, z) + offset,
                new Vector3(xu, yu, zu) + offset,
                new Vector3(x, yu, zu) + offset,
                Color.Yellow
            );
        }

        if ((yd >= 0 ? chunk[false, x, (byte)yd, z] : 0) == 0) {
            builder.Quad(
                new Vector3(x, y, zu) + offset,
                new Vector3(xu, y, zu) + offset,
                new Vector3(xu, y, z) + offset,
                new Vector3(x, y, z) + offset,
                Color.Green
            );
        }

        if ((xd >= 0 ? chunk[false, (byte)xd, y, z] : w[false, 31, y, z]) == 0) {
            builder.Quad(
                new Vector3(x, y, z) + offset,
                new Vector3(x, yu, z) + offset,
                new Vector3(x, yu, zu) + offset,
                new Vector3(x, y, zu) + offset,
                Color.Blue
            );
        }


        if ((xu <= 31 ? chunk[false, (byte)xu, y, z] : e[false, 0, y, z]) == 0) {
            builder.Quad(
                new Vector3(xu, y, zu) + offset,
                new Vector3(xu, yu, zu) + offset,
                new Vector3(xu, yu, z) + offset,
                new Vector3(xu, y, z) + offset,
                Color.Purple
            );
        }
    }
}
