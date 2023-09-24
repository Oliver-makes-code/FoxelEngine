using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Voxel.Client.World;
using Voxel.Common.World;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Voxel.Client.Rendering;

public class ChunkMesh {
    public int primitiveCount;
    VertexBuffer? vertices = null;
    public const float AO_STEP = 0.1f;

    public ChunkMesh() {}

    public void Draw(GraphicsDevice device, Effect effect, Vector3 pos, Camera camera, List<(Vector2, string)> points) {
        if (vertices == null)
            return;

        device.SetVertexBuffer(vertices);

        effect.CurrentTechnique.Passes[0].Apply();

        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
        var chunkDrawPos = pos + new Vector3(16, 32, 16);
        if (!camera.IsPointVisible(chunkDrawPos))
            return;

        Viewport viewport = device.Viewport;
        var point = viewport.Project(chunkDrawPos, camera.Projection, camera.View, camera.World);
        var screenPoint = new Vector2(point.X, point.Y);

        points.Add((screenPoint, $"{new BlockPos(pos).ChunkPos()}"));
    }

    public void BuildChunk(GraphicsDevice device, ClientWorld world, ChunkPos pos) {
        MeshBuilder builder = new();
        ChunkView view = new(world.world, pos);

        for (byte x = 0; x < 0b10_0000u; x++) {
            for (byte y = 0; y < 0b10_0000u; y++) {
                for (byte z = 0; z < 0b10_0000u; z++) {
                    GenerateCube(builder, view, new BlockPos(pos, new(false, x, y, z)));
                }
            }
        }
        
        var mesh = builder.Build();

        if (mesh.vertices.Length != 0) {
            // Use temporary variable to avoid drawing while data is being written off-thread
            var temp_vertices = new VertexBuffer(device, typeof(VertexPositionColorTexture), mesh.vertices.Length, BufferUsage.WriteOnly);
            temp_vertices.SetData(mesh.vertices);

            vertices = temp_vertices;
            primitiveCount = mesh.vertices.Length/2;
        } else {
            vertices = null;
        }
    }

    public void GenerateCube(MeshBuilder builder, ChunkView world, BlockPos pos) {
        var block = world.GetBlock(pos);
        if (block == 0)
            return;
        
        var blocks = new ushort[3, 3, 3];
        var emptyBlocks = new bool[3, 3, 3];
        var aoBlocks = new byte[3, 3, 3];
        
        for (var x = 0; x < 3; x++) {
            for (var y = 0; y < 3; y++) {
                for (var z = 0; z < 3; z++) {
                    var _block = world.GetBlock(pos + new BlockPos(x-1, y-1, z-1));
                    var empty = _block == 0;
                    blocks[x,y,z] = _block;
                    emptyBlocks[x,y,z] = empty;
                    aoBlocks[x, y, z] = empty ? (byte)0 : (byte)1;
                }
            }
        }
        
        var tx = 0;
        
        if (emptyBlocks[1, 1, 0]) {
            var v1 = 1 - AO_STEP * (aoBlocks[0, 1, 0] + aoBlocks[1, 0, 0] + aoBlocks[0, 0, 0]);
            var v2 = 1 - AO_STEP * (aoBlocks[2, 1, 0] + aoBlocks[1, 0, 0] + aoBlocks[2, 0, 0]);
            var v3 = 1 - AO_STEP * (aoBlocks[2, 1, 0] + aoBlocks[1, 2, 0] + aoBlocks[2, 2, 0]);
            var v4 = 1 - AO_STEP * (aoBlocks[0, 1, 0] + aoBlocks[1, 2, 0] + aoBlocks[0, 2, 0]);
            
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
        
        if (emptyBlocks[1, 1, 2]) {
            var v1 = 1 - AO_STEP * (aoBlocks[2, 1, 2] + aoBlocks[1, 0, 2] + aoBlocks[2, 0, 2]);
            var v2 = 1 - AO_STEP * (aoBlocks[0, 1, 2] + aoBlocks[1, 0, 2] + aoBlocks[0, 0, 2]);
            var v3 = 1 - AO_STEP * (aoBlocks[0, 1, 2] + aoBlocks[1, 2, 2] + aoBlocks[0, 2, 2]);
            var v4 = 1 - AO_STEP * (aoBlocks[2, 1, 2] + aoBlocks[1, 2, 2] + aoBlocks[2, 2, 2]);
        
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
        
        if (emptyBlocks[1, 2, 1]) {
            var v1 = 1 - AO_STEP * (aoBlocks[0, 2, 1] + aoBlocks[1, 2, 0] + aoBlocks[0, 2, 0]); // NW
            var v2 = 1 - AO_STEP * (aoBlocks[2, 2, 1] + aoBlocks[1, 2, 0] + aoBlocks[2, 2, 0]); // NE
            var v3 = 1 - AO_STEP * (aoBlocks[2, 2, 1] + aoBlocks[1, 2, 2] + aoBlocks[2, 2, 2]); // SE
            var v4 = 1 - AO_STEP * (aoBlocks[0, 2, 1] + aoBlocks[1, 2, 2] + aoBlocks[0, 2, 2]); // SW
        
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
        
        if (emptyBlocks[1, 0, 1]) {
            var v1 = 1 - AO_STEP * (aoBlocks[0, 0, 1] + aoBlocks[1, 0, 2] + aoBlocks[0, 0, 2]); // SW
            var v2 = 1 - AO_STEP * (aoBlocks[2, 0, 1] + aoBlocks[1, 0, 2] + aoBlocks[2, 0, 2]); // SE
            var v3 = 1 - AO_STEP * (aoBlocks[2, 0, 1] + aoBlocks[1, 0, 0] + aoBlocks[2, 0, 0]); // NE
            var v4 = 1 - AO_STEP * (aoBlocks[0, 0, 1] + aoBlocks[1, 0, 0] + aoBlocks[0, 0, 0]); // NW
          
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
        
        if (emptyBlocks[0, 1, 1]) {
            var v1 = 1 - AO_STEP * (aoBlocks[0, 0, 1] + aoBlocks[0, 1, 0] + aoBlocks[0, 0, 0]);
            var v2 = 1 - AO_STEP * (aoBlocks[0, 2, 1] + aoBlocks[0, 1, 0] + aoBlocks[0, 2, 0]);
            var v3 = 1 - AO_STEP * (aoBlocks[0, 2, 1] + aoBlocks[0, 1, 2] + aoBlocks[0, 2, 2]);
            var v4 = 1 - AO_STEP * (aoBlocks[0, 0, 1] + aoBlocks[0, 1, 2] + aoBlocks[0, 0, 2]);
        
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
        
        if (emptyBlocks[2, 1, 1]) {
            var v1 = 1 - AO_STEP * (aoBlocks[2, 0, 1] + aoBlocks[2, 1, 2] + aoBlocks[2, 0, 2]);
            var v2 = 1 - AO_STEP * (aoBlocks[2, 2, 1] + aoBlocks[2, 1, 2] + aoBlocks[2, 2, 2]);
            var v3 = 1 - AO_STEP * (aoBlocks[2, 2, 1] + aoBlocks[2, 1, 0] + aoBlocks[2, 2, 0]);
            var v4 = 1 - AO_STEP * (aoBlocks[2, 0, 1] + aoBlocks[2, 1, 0] + aoBlocks[2, 0, 0]);
        
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
