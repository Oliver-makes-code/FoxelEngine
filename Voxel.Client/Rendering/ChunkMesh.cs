using System;
using Microsoft.Xna.Framework.Graphics;
using Voxel.Client.World;
using Voxel.Common.World;

namespace Voxel.Client.Rendering;

public class ChunkMesh {
    public int primitiveCount;
    VertexBuffer? vertices = null;
    IndexBuffer? indices = null;
    static float AO_STEP = 0.1f;

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
                    GenerateCube(builder, world, new BlockPos(pos, new(false, x, y, z)));
                }
            }
        }
    }

    public void GenerateCube(MeshBuilder builder, ClientWorld world, BlockPos pos) {
        var block = world.world.GetBlock(pos);
        if (block == 0)
            return;

        var xd = new BlockPos(-1, 0, 0);
        var xu = new BlockPos(1, 0, 0);
        var yd = new BlockPos(0, -1, 0);
        var yu = new BlockPos(0, 1, 0);
        var zd = new BlockPos(0, 0, -1);
        var zu = new BlockPos(0, 0, 1);

        var tx = Random.Shared.Next(3);
        if (tx == 2)
            tx = 3;

        tx *= 16;

        if (world.world.GetBlock(pos + zd) == 0) {
            var bxu = world.world.GetBlock(pos + zd + xu) == 0 ? 0 : 1;
            var bxd = world.world.GetBlock(pos + zd + xd) == 0 ? 0 : 1;
            var byu = world.world.GetBlock(pos + zd + yu) == 0 ? 0 : 1;
            var byd = world.world.GetBlock(pos + zd + yd) == 0 ? 0 : 1;
            var bxuyd = world.world.GetBlock(pos + zd + xu + yd) == 0 ? 0 : 1;
            var bxuyu = world.world.GetBlock(pos + zd + xu + yu) == 0 ? 0 : 1;
            var bxdyd = world.world.GetBlock(pos + zd + xd + yd) == 0 ? 0 : 1;
            var bxdyu = world.world.GetBlock(pos + zd + xd + yu) == 0 ? 0 : 1;

            var br = 1 - AO_STEP * (bxd + byd + bxdyd);
            var bl = 1 - AO_STEP * (bxu + byd + bxuyd);
            var tl = 1 - AO_STEP * (bxu + byu + bxuyu);
            var tr = 1 - AO_STEP * (bxd + byu + bxdyu);
            
            builder.Quad(
                new(
                    pos.vector3,
                    new(br, br, br),
                    new(tx+15.99f, 15.99f)
                ),
                new(
                    (pos + xu).vector3,
                    new(bl, bl, bl),
                    new(tx, 15.99f)
                ),
                new(
                    (pos + xu + yu).vector3,
                    new(tl, tl, tl),
                    new(tx, 0)
                ),
                new(
                    (pos + yu).vector3,
                    new(tr, tr, tr),
                    new(tx+15.99f, 0)
                )
            );
        }

        if (world.world.GetBlock(pos + zu) == 0) {
            var bxu = world.world.GetBlock(pos + zu + xu) == 0 ? 0 : 1;
            var bxd = world.world.GetBlock(pos + zu + xd) == 0 ? 0 : 1;
            var byu = world.world.GetBlock(pos + zu + yu) == 0 ? 0 : 1;
            var byd = world.world.GetBlock(pos + zu + yd) == 0 ? 0 : 1;
            var bxuyd = world.world.GetBlock(pos + zu + xu + yd) == 0 ? 0 : 1;
            var bxuyu = world.world.GetBlock(pos + zu + xu + yu) == 0 ? 0 : 1;
            var bxdyd = world.world.GetBlock(pos + zu + xd + yd) == 0 ? 0 : 1;
            var bxdyu = world.world.GetBlock(pos + zu + xd + yu) == 0 ? 0 : 1;

            var br = 1 - AO_STEP * (bxu + byd + bxuyd);
            var bl = 1 - AO_STEP * (bxd + byd + bxdyd);
            var tl = 1 - AO_STEP * (bxd + byu + bxdyu);
            var tr = 1 - AO_STEP * (bxu + byu + bxuyu);
            
            builder.Quad(
                new(
                    (pos + zu).vector3,
                    new(bl, bl, bl),
                    new(tx, 15.99f)
                ),
                new(
                    (pos + yu + zu).vector3,
                    new(tl, tl, tl),
                    new(tx, 0)
                ),
                new(
                    (pos + xu + yu + zu).vector3,
                    new(tr, tr, tr),
                    new(tx+15.99f, 0)
                ),
                new(
                    (pos + xu + zu).vector3,
                    new(br, br, br),
                    new(tx+15.99f, 15.99f)
                )
            );
        }
        
        if (world.world.GetBlock(pos + yu) == 0) {
            var bxu = world.world.GetBlock(pos + yu + xu) == 0 ? 0 : 1;
            var bxd = world.world.GetBlock(pos + yu + xd) == 0 ? 0 : 1;
            var bzu = world.world.GetBlock(pos + yu + zu) == 0 ? 0 : 1;
            var bzd = world.world.GetBlock(pos + yu + zd) == 0 ? 0 : 1;
            var bxuzd = world.world.GetBlock(pos + yu + xu + zd) == 0 ? 0 : 1;
            var bxuzu = world.world.GetBlock(pos + yu + xu + zu) == 0 ? 0 : 1;
            var bxdzd = world.world.GetBlock(pos + yu + xd + zd) == 0 ? 0 : 1;
            var bxdzu = world.world.GetBlock(pos + yu + xd + zu) == 0 ? 0 : 1;

            var v1 = 1 - AO_STEP * (bxd + bzd + bxdzd); // NW
            var v2 = 1 - AO_STEP * (bxu + bzd + bxuzd); // NE
            var v3 = 1 - AO_STEP * (bxu + bzu + bxuzu); // SE
            var v4 = 1 - AO_STEP * (bxd + bzu + bxdzu); // SW

            builder.Quad(
                new(
                    (pos + yu).vector3,
                    new(v1, v1, v1),
                    new(tx+15.99f, 15.99f)
                ),
                new(
                    (pos + xu + yu).vector3,
                    new(v2, v2, v2),
                    new(tx, 15.99f)
                ),
                new(
                    (pos + xu + yu + zu).vector3,
                    new(v3, v3, v3),
                    new(tx, 0)
                ),
                new(
                    (pos + yu + zu).vector3,
                    new(v4, v4, v4),
                    new(tx+15.99f, 0)
                )
            );
        }
        
        if (world.world.GetBlock(pos + yd) == 0) {
            var bxu = world.world.GetBlock(pos + yd + xu) == 0 ? 0 : 1;
            var bxd = world.world.GetBlock(pos + yd + xd) == 0 ? 0 : 1;
            var bzu = world.world.GetBlock(pos + yd + zu) == 0 ? 0 : 1;
            var bzd = world.world.GetBlock(pos + yd + zd) == 0 ? 0 : 1;
            var bxuzd = world.world.GetBlock(pos + yd + xu + zd) == 0 ? 0 : 1;
            var bxuzu = world.world.GetBlock(pos + yd + xu + zu) == 0 ? 0 : 1;
            var bxdzd = world.world.GetBlock(pos + yd + xd + zd) == 0 ? 0 : 1;
            var bxdzu = world.world.GetBlock(pos + yd + xd + zu) == 0 ? 0 : 1;

            var v1 = 1 - AO_STEP * (bxd + bzu + bxdzu); // SW
            var v2 = 1 - AO_STEP * (bxu + bzu + bxuzu); // SE
            var v3 = 1 - AO_STEP * (bxu + bzd + bxuzd); // NE
            var v4 = 1 - AO_STEP * (bxd + bzd + bxdzd); // NW
            
            builder.Quad(
                new(
                    (pos + zu).vector3,
                    new(v1, v1, v1),
                    new(tx, 0)
                ),
                new(
                    (pos + xu + zu).vector3,
                    new(v2, v2, v2),
                    new(tx+15.99f, 0)
                ),
                new(
                    (pos + xu).vector3,
                    new(v3, v3, v3),
                    new(tx+15.99f, 15.99f)
                ),
                new(
                    pos.vector3,
                    new(v4, v4, v4),
                    new(tx, 15.99f)
                )
            );
        }
        
        if (world.world.GetBlock(pos + xd) == 0) {
            var byu = world.world.GetBlock(pos + xd + yu) == 0 ? 0 : 1;
            var byd = world.world.GetBlock(pos + xd + yd) == 0 ? 0 : 1;
            var bzu = world.world.GetBlock(pos + xd + zu) == 0 ? 0 : 1;
            var bzd = world.world.GetBlock(pos + xd + zd) == 0 ? 0 : 1;
            var byuzd = world.world.GetBlock(pos + xd + yu + zd) == 0 ? 0 : 1;
            var byuzu = world.world.GetBlock(pos + xd + yu + zu) == 0 ? 0 : 1;
            var bydzd = world.world.GetBlock(pos + xd + yd + zd) == 0 ? 0 : 1;
            var bydzu = world.world.GetBlock(pos + xd + yd + zu) == 0 ? 0 : 1;

            var v1 = 1 - AO_STEP * (byd + bzd + bydzd);
            var v2 = 1 - AO_STEP * (byu + bzd + byuzd);
            var v3 = 1 - AO_STEP * (byu + bzu + byuzu);
            var v4 = 1 - AO_STEP * (byd + bzu + bydzu);

            builder.Quad(
                new(
                    pos.vector3,
                    new(v1, v1, v1),
                    new(tx, 15.99f)
                ),
                new(
                    (pos + yu).vector3,
                    new(v2, v2, v2),
                    new(tx, 0)
                ),
                new(
                    (pos + yu + zu).vector3,
                    new(v3, v3, v3),
                    new(tx+15.99f, 0)
                ),
                new(
                    (pos + zu).vector3,
                    new(v4, v4, v4),
                    new(tx+15.99f, 15.99f)
                )
            );
        }
        
        
        if (world.world.GetBlock(pos + xu) == 0) {
            var byu = world.world.GetBlock(pos + xu + yu) == 0 ? 0 : 1;
            var byd = world.world.GetBlock(pos + xu + yd) == 0 ? 0 : 1;
            var bzu = world.world.GetBlock(pos + xu + zu) == 0 ? 0 : 1;
            var bzd = world.world.GetBlock(pos + xu + zd) == 0 ? 0 : 1;
            var byuzd = world.world.GetBlock(pos + xu + yu + zd) == 0 ? 0 : 1;
            var byuzu = world.world.GetBlock(pos + xu + yu + zu) == 0 ? 0 : 1;
            var bydzd = world.world.GetBlock(pos + xu + yd + zd) == 0 ? 0 : 1;
            var bydzu = world.world.GetBlock(pos + xu + yd + zu) == 0 ? 0 : 1;

            var v1 = 1 - AO_STEP * (byd + bzu + bydzu);
            var v2 = 1 - AO_STEP * (byu + bzu + byuzu);
            var v3 = 1 - AO_STEP * (byu + bzd + byuzd);
            var v4 = 1 - AO_STEP * (byd + bzd + bydzd);

            builder.Quad(
                new(
                    (pos + xu + zu).vector3,
                    new(v1, v1, v1),
                    new(tx, 15.99f)
                ),
                new(
                    (pos + xu + yu + zu).vector3,
                    new(v2, v2, v2),
                    new(tx, 0)
                ),
                new(
                    (pos + xu + yu).vector3,
                    new(v3, v3, v3),
                    new(tx+15.99f, 0)
                ),
                new(
                    (pos + xu).vector3,
                    new(v4, v4, v4),
                    new(tx+15.99f, 15.99f)
                )
            );
        }
    }
}
