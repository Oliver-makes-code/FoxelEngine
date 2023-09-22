using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxel.Client.World;
using Voxel.Common.World;

namespace Voxel.Client.Rendering;

public class ChunkMesh {
    public int primitiveCount;
    VertexBuffer? vertices = null;
    IndexBuffer? indices = null;

    public ChunkMesh(GraphicsDevice device, ClientWorld world, ChunkPos pos) {
        BuildChunk(device, world, pos);
    }

    public void Draw(GraphicsDevice device, Effect effect) {
        if (vertices == null)
            return;

        device.SetVertexBuffer(vertices);
        device.Indices = indices;

        effect.CurrentTechnique.Passes[0].Apply();

        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
    }

    public void BuildChunk(GraphicsDevice device, ClientWorld world, ChunkPos pos) {
        var mesh = BuildChunk(world, pos);
        if (mesh.vertices.Length != 0) {
            vertices = new(device, typeof(VertexPositionColorTexture), mesh.vertices.Length, BufferUsage.WriteOnly);
            indices = new(device, IndexElementSize.ThirtyTwoBits, mesh.indices.Length, BufferUsage.WriteOnly);
            vertices.SetData(mesh.vertices);
            indices.SetData(mesh.indices);
        } else {
            vertices = null;
            indices = null;
        }

        primitiveCount = mesh.indices.Length / 3;
    }

    public Mesh BuildChunk(ClientWorld world, ChunkPos pos) {
        MeshBuilder builder = new();
        BuildChunk(world, pos, builder);
        return builder.Build();
    }

    public void BuildChunk(ClientWorld world, ChunkPos pos, MeshBuilder builder) {
        var chunk = world.world[pos] ?? Chunk.Empty;
        var n = world.world[pos.North()] ?? Chunk.Empty;
        var s = world.world[pos.South()] ?? Chunk.Empty;
        var e = world.world[pos.East()] ?? Chunk.Empty;
        var w = world.world[pos.West()] ?? Chunk.Empty;
        var u = world.world[pos.Up()] ?? Chunk.Empty;
        var d = world.world[pos.Down()] ?? Chunk.Empty;
        for (byte x = 0; x < 0b10_0000u; x++) {
            for (byte y = 0; y < 0b10_0000u; y++) {
                for (byte z = 0; z < 0b10_0000u; z++) {
                    GenerateCube(builder, chunk, pos, x, y, z, n, s, e, w, u, d);
                }
            }
        }
    }

    public void GenerateCube(MeshBuilder builder, Chunk chunk, ChunkPos pos, byte x, byte y, byte z, Chunk n, Chunk s, Chunk e, Chunk w, Chunk u, Chunk d) {
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

        var tx = Random.Shared.Next(3);
        if (tx == 2)
            tx = 3;

        tx *= 16;

        if ((zd >= 0 ? chunk[false, x, y, (byte)zd] : n[false, x, y, 31]) == 0) {
            var color = Color.Red;
            builder.Quad(
                new(
                    new Vector3(x, y, z) + offset,
                    color,
                    new(tx+15.99f, 15.99f)
                ),
                new(
                    new Vector3(xu, y, z) + offset,
                    color,
                    new(tx, 15.99f)
                ),
                new(
                    new Vector3(xu, yu, z) + offset,
                    color,
                    new(tx, 0)
                ),
                new(
                    new Vector3(x, yu, z) + offset,
                    color,
                    new(tx+15.99f, 0)
                )
            );
        }

        if ((zu <= 31 ? chunk[false, x, y, (byte)zu] : s[false, x, y, 0]) == 0) {
            var color = Color.Orange;
            builder.Quad(
                new(
                    new Vector3(x, y, zu) + offset,
                    color,
                    new(tx, 15.99f)
                ),
                new(
                    new Vector3(x, yu, zu) + offset,
                    color,
                    new(tx, 0)
                ),
                new(
                    new Vector3(xu, yu, zu) + offset,
                    color,
                    new(tx+15.99f, 0)
                ),
                new(
                    new Vector3(xu, y, zu) + offset,
                    color,
                    new(tx+15.99f, 15.99f)
                )
            );
        }

        if ((yu <= 31 ? chunk[false, x, (byte)yu, z] : u[false, x, 0, z]) == 0) {
            var color = Color.Yellow;
            builder.Quad(
                new(
                    new Vector3(x, yu, z) + offset,
                    color,
                    new(tx+15.99f, 15.99f)
                ),
                new(
                    new Vector3(xu, yu, z) + offset,
                    color,
                    new(tx, 15.99f)
                ),
                new(
                    new Vector3(xu, yu, zu) + offset,
                    color,
                    new(tx, 0)
                ),
                new(
                    new Vector3(x, yu, zu) + offset,
                    color,
                    new(tx+15.99f, 0)
                )
            );
        }

        if ((yd >= 0 ? chunk[false, x, (byte)yd, z] : d[false, x, 31, z]) == 0) {
            var color = Color.Green;
            builder.Quad(
                new(
                    new Vector3(x, y, zu) + offset,
                    color,
                    new(tx, 0)
                ),
                new(
                    new Vector3(xu, y, zu) + offset,
                    color,
                    new(tx+15.99f, 0)
                ),
                new(
                    new Vector3(xu, y, z) + offset,
                    color,
                    new(tx+15.99f, 15.99f)
                ),
                new(
                    new Vector3(x, y, z) + offset,
                    color,
                    new(tx, 15.99f)
                )
            );
        }

        if ((xd >= 0 ? chunk[false, (byte)xd, y, z] : w[false, 31, y, z]) == 0) {
            var color = Color.Blue;
            builder.Quad(
                new(
                    new Vector3(x, y, z) + offset,
                    color,
                    new(tx, 15.99f)
                ),
                new(
                    new Vector3(x, yu, z) + offset,
                    color,
                    new(tx, 0)
                ),
                new(
                    new Vector3(x, yu, zu) + offset,
                    color,
                    new(tx+15.99f, 0)
                ),
                new(
                    new Vector3(x, y, zu) + offset,
                    color,
                    new(tx+15.99f, 15.99f)
                )
            );
        }


        if ((xu <= 31 ? chunk[false, (byte)xu, y, z] : e[false, 0, y, z]) == 0) {
            var color = Color.Purple;
            builder.Quad(
                new(
                    new Vector3(xu, y, zu) + offset,
                    color,
                    new(tx, 15.99f)
                ),
                new(
                    new Vector3(xu, yu, zu) + offset,
                    color,
                    new(tx, 0)
                ),
                new(
                    new Vector3(xu, yu, z) + offset,
                    color,
                    new(tx+15.99f, 0)
                ),
                new(
                    new Vector3(xu, y, z) + offset,
                    color,
                    new(tx+15.99f, 15.99f)
                )
            );
        }
    }
}
