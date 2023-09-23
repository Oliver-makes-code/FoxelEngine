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
        ChunkView view = new(world.world, pos);

        for (byte x = 0; x < 0b10_0000u; x++) {
            for (byte y = 0; y < 0b10_0000u; y++) {
                for (byte z = 0; z < 0b10_0000u; z++) {
                    GenerateCube(builder, view, new BlockPos(pos, new(false, x, y, z)));
                }
            }
        }
    }

    public void GenerateCube(MeshBuilder builder, ChunkView world, BlockPos pos) {
        var block = world.GetBlock(pos);
        if (block == 0)
            return;

        var blocks = new byte[3, 3, 3];
        
        for (int x = 0; x < 3; x++) {
            for (int y = 0; y < 3; y++) {
                for (int z = 0; z < 3; z++) {
                    blocks[x, y, z] = world.GetBlock(pos + new BlockPos(x-1, y-1, z-1)) == 0 ? (byte)0 : (byte)1;
                }
            }
        }

        var tx = Random.Shared.Next(3);
        if (tx == 2)
            tx = 3;

        tx *= 16;

        if (blocks[1, 1, 0] == 0) {
            var v1 = 1 - AO_STEP * (blocks[0, 1, 0] + blocks[1, 0, 0] + blocks[0, 0, 0]);
            var v2 = 1 - AO_STEP * (blocks[2, 1, 0] + blocks[1, 0, 0] + blocks[2, 0, 0]);
            var v3 = 1 - AO_STEP * (blocks[2, 1, 0] + blocks[1, 2, 0] + blocks[2, 2, 0]);
            var v4 = 1 - AO_STEP * (blocks[0, 1, 0] + blocks[1, 2, 0] + blocks[0, 2, 0]);
            
            builder.Quad(
                new(
                    pos.vector3,
                    new(v1, v1, v1),
                    new(tx+15.99f, 15.99f)
                ),
                new(
                    (pos + BlockPos.East).vector3,
                    new(v2, v2, v2),
                    new(tx, 15.99f)
                ),
                new(
                    (pos + BlockPos.East + BlockPos.Up).vector3,
                    new(v3, v3, v3),
                    new(tx, 0)
                ),
                new(
                    (pos + BlockPos.Up).vector3,
                    new(v4, v4, v4),
                    new(tx+15.99f, 0)
                )
            );
        }

        if (blocks[1, 1, 2] == 0) {
            var v1 = 1 - AO_STEP * (blocks[2, 1, 2] + blocks[1, 0, 2] + blocks[2, 0, 2]);
            var v2 = 1 - AO_STEP * (blocks[0, 1, 2] + blocks[1, 0, 2] + blocks[0, 0, 2]);
            var v3 = 1 - AO_STEP * (blocks[0, 1, 2] + blocks[1, 2, 2] + blocks[0, 2, 2]);
            var v4 = 1 - AO_STEP * (blocks[2, 1, 2] + blocks[1, 2, 2] + blocks[2, 2, 2]);
            
            builder.Quad(
                new(
                    (pos + BlockPos.South).vector3,
                    new(v2, v2, v2),
                    new(tx, 15.99f)
                ),
                new(
                    (pos + BlockPos.Up + BlockPos.South).vector3,
                    new(v3, v3, v3),
                    new(tx, 0)
                ),
                new(
                    (pos + BlockPos.East + BlockPos.Up + BlockPos.South).vector3,
                    new(v4, v4, v4),
                    new(tx+15.99f, 0)
                ),
                new(
                    (pos + BlockPos.East + BlockPos.South).vector3,
                    new(v1, v1, v1),
                    new(tx+15.99f, 15.99f)
                )
            );
        }
        
        if (blocks[1, 2, 1] == 0) {
            var v1 = 1 - AO_STEP * (blocks[0, 2, 1] + blocks[1, 2, 0] + blocks[0, 2, 0]); // NW
            var v2 = 1 - AO_STEP * (blocks[2, 2, 1] + blocks[1, 2, 0] + blocks[2, 2, 0]); // NE
            var v3 = 1 - AO_STEP * (blocks[2, 2, 1] + blocks[1, 2, 2] + blocks[2, 2, 2]); // SE
            var v4 = 1 - AO_STEP * (blocks[0, 2, 1] + blocks[1, 2, 2] + blocks[0, 2, 2]); // SW

            builder.Quad(
                new(
                    (pos + BlockPos.Up).vector3,
                    new(v1, v1, v1),
                    new(tx+15.99f, 15.99f)
                ),
                new(
                    (pos + BlockPos.East + BlockPos.Up).vector3,
                    new(v2, v2, v2),
                    new(tx, 15.99f)
                ),
                new(
                    (pos + BlockPos.East + BlockPos.Up + BlockPos.South).vector3,
                    new(v3, v3, v3),
                    new(tx, 0)
                ),
                new(
                    (pos + BlockPos.Up + BlockPos.South).vector3,
                    new(v4, v4, v4),
                    new(tx+15.99f, 0)
                )
            );
        }
        
        if (blocks[1, 0, 1] == 0) {
            var v1 = 1 - AO_STEP * (blocks[0, 0, 1] + blocks[1, 0, 2] + blocks[0, 0, 2]); // SW
            var v2 = 1 - AO_STEP * (blocks[2, 0, 1] + blocks[1, 0, 2] + blocks[2, 0, 2]); // SE
            var v3 = 1 - AO_STEP * (blocks[2, 0, 1] + blocks[1, 0, 0] + blocks[2, 0, 0]); // NE
            var v4 = 1 - AO_STEP * (blocks[0, 0, 1] + blocks[1, 0, 0] + blocks[0, 0, 0]); // NW
            
            builder.Quad(
                new(
                    (pos + BlockPos.South).vector3,
                    new(v1, v1, v1),
                    new(tx, 0)
                ),
                new(
                    (pos + BlockPos.East + BlockPos.South).vector3,
                    new(v2, v2, v2),
                    new(tx+15.99f, 0)
                ),
                new(
                    (pos + BlockPos.East).vector3,
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
        
        if (blocks[0, 1, 1] == 0) {
            var v1 = 1 - AO_STEP * (blocks[0, 0, 1] + blocks[0, 1, 0] + blocks[0, 0, 0]);
            var v2 = 1 - AO_STEP * (blocks[0, 2, 1] + blocks[0, 1, 0] + blocks[0, 2, 0]);
            var v3 = 1 - AO_STEP * (blocks[0, 2, 1] + blocks[0, 1, 2] + blocks[0, 2, 2]);
            var v4 = 1 - AO_STEP * (blocks[0, 0, 1] + blocks[0, 1, 2] + blocks[0, 0, 2]);

            builder.Quad(
                new(
                    pos.vector3,
                    new(v1, v1, v1),
                    new(tx, 15.99f)
                ),
                new(
                    (pos + BlockPos.Up).vector3,
                    new(v2, v2, v2),
                    new(tx, 0)
                ),
                new(
                    (pos + BlockPos.Up + BlockPos.South).vector3,
                    new(v3, v3, v3),
                    new(tx+15.99f, 0)
                ),
                new(
                    (pos + BlockPos.South).vector3,
                    new(v4, v4, v4),
                    new(tx+15.99f, 15.99f)
                )
            );
        }
        
        
        if (blocks[2, 1, 1] == 0) {
            var v1 = 1 - AO_STEP * (blocks[2, 0, 1] + blocks[2, 1, 2] + blocks[2, 0, 2]);
            var v2 = 1 - AO_STEP * (blocks[2, 2, 1] + blocks[2, 1, 2] + blocks[2, 2, 2]);
            var v3 = 1 - AO_STEP * (blocks[2, 2, 1] + blocks[2, 1, 0] + blocks[2, 2, 0]);
            var v4 = 1 - AO_STEP * (blocks[2, 0, 1] + blocks[2, 1, 0] + blocks[2, 0, 0]);

            builder.Quad(
                new(
                    (pos + BlockPos.East + BlockPos.South).vector3,
                    new(v1, v1, v1),
                    new(tx, 15.99f)
                ),
                new(
                    (pos + BlockPos.East + BlockPos.Up + BlockPos.South).vector3,
                    new(v2, v2, v2),
                    new(tx, 0)
                ),
                new(
                    (pos + BlockPos.East + BlockPos.Up).vector3,
                    new(v3, v3, v3),
                    new(tx+15.99f, 0)
                ),
                new(
                    (pos + BlockPos.East).vector3,
                    new(v4, v4, v4),
                    new(tx+15.99f, 15.99f)
                )
            );
        }
    }
}
