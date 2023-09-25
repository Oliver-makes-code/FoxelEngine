using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voxel.Client.World;
using Voxel.Common.Tile;
using Voxel.Common.World;

namespace Voxel.Client.Rendering;

public class ChunkMesh {
    public const float AO_STEP = 0.1f;
    public const uint CHUNK_SIZE = 32;
    public const float TEXTIRE_START = 0.1f;
    public const float TEXTURE_SIZE = 15.9f;
    
    private static BlockPos[] normals = { BlockPos.East, BlockPos.West, BlockPos.Up, BlockPos.Down, BlockPos.South, BlockPos.North };
    private static BlockPos[,] vertexOffsets = {
        { BlockPos.East + BlockPos.South, BlockPos.East + BlockPos.Up + BlockPos.South, BlockPos.East + BlockPos.Up, BlockPos.East }, // East
        { BlockPos.Empty, BlockPos.Up, BlockPos.Up + BlockPos.South, BlockPos.South }, // West
        { BlockPos.Up, BlockPos.East + BlockPos.Up, BlockPos.East + BlockPos.Up + BlockPos.South, BlockPos.Up + BlockPos.South }, // Up
        { BlockPos.South, BlockPos.East + BlockPos.South, BlockPos.East, BlockPos.Empty }, // Down
        { BlockPos.South, BlockPos.Up + BlockPos.South, BlockPos.East + BlockPos.Up + BlockPos.South, BlockPos.East + BlockPos.South }, // South
        { BlockPos.Empty, BlockPos.East, BlockPos.East + BlockPos.Up, BlockPos.Up } // North
    };
    private static float[,,] textureCoords = {
        { { TEXTIRE_START, TEXTURE_SIZE }, { TEXTIRE_START, TEXTIRE_START }, { TEXTURE_SIZE, TEXTIRE_START }, { TEXTURE_SIZE, TEXTURE_SIZE } }, // East
        { { TEXTIRE_START, TEXTURE_SIZE }, { TEXTIRE_START, TEXTIRE_START }, { TEXTURE_SIZE, TEXTIRE_START }, { TEXTURE_SIZE, TEXTURE_SIZE } }, // West
        { { TEXTURE_SIZE, TEXTURE_SIZE }, { TEXTIRE_START, TEXTURE_SIZE }, { TEXTIRE_START, TEXTIRE_START }, { TEXTURE_SIZE, TEXTIRE_START } }, // Up
        { { TEXTIRE_START, TEXTIRE_START }, { TEXTURE_SIZE, TEXTIRE_START }, { TEXTURE_SIZE, TEXTURE_SIZE }, { TEXTIRE_START, TEXTURE_SIZE } }, // Down
        { { TEXTIRE_START, TEXTURE_SIZE }, { TEXTIRE_START, TEXTIRE_START }, { TEXTURE_SIZE, TEXTIRE_START }, { TEXTURE_SIZE, TEXTURE_SIZE } }, // South
        { { TEXTURE_SIZE, TEXTURE_SIZE }, { TEXTIRE_START, TEXTURE_SIZE }, { TEXTIRE_START, TEXTIRE_START }, { TEXTURE_SIZE, TEXTIRE_START } }  // North
    };
    private static BlockPos[,,] aoOffsets = {
        { { BlockPos.Down, BlockPos.South }, { BlockPos.Up, BlockPos.South }, { BlockPos.Up, BlockPos.North }, { BlockPos.Down, BlockPos.North } }, // East
        { { BlockPos.Down, BlockPos.North }, { BlockPos.Up, BlockPos.North }, { BlockPos.Up, BlockPos.South }, { BlockPos.Down, BlockPos.South } }, // West
        { { BlockPos.West, BlockPos.North }, { BlockPos.East, BlockPos.North }, { BlockPos.East, BlockPos.South }, { BlockPos.West, BlockPos.South } }, // Up
        { { BlockPos.West, BlockPos.South }, { BlockPos.East, BlockPos.South }, { BlockPos.East, BlockPos.North }, { BlockPos.West, BlockPos.North } }, // Down
        { { BlockPos.East, BlockPos.Down }, { BlockPos.West, BlockPos.Down }, { BlockPos.West, BlockPos.Up }, { BlockPos.East, BlockPos.Up } }, // South
        { { BlockPos.West, BlockPos.Down }, { BlockPos.East, BlockPos.Down }, { BlockPos.East, BlockPos.Up }, { BlockPos.West, BlockPos.Up } }  // North
    };
    
    public int primitiveCount;
    VertexBuffer? vertices;

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
        var watch = new Stopwatch();
        watch.Start();
        MeshBuilder builder = new();
        ChunkView view = new(world.world, pos);

        for (var x = 0; x < CHUNK_SIZE; x++) {
            for (var y = 0; y < CHUNK_SIZE; y++) {
                for (var z = 0; z < CHUNK_SIZE; z++) {
                    ChunkBlockPos chunkPos = new(false, x, y, z);
                    var blockPos = new BlockPos(ref pos, ref chunkPos);
                    var block = view.GetBlock(blockPos);

                    if (!block.IsSolidBlock) {
                        continue;
                    }

                    for (var direction = 0; direction < 6; direction++) {
                        GenerateQuad(builder, view, blockPos, direction);
                    }
                }
            }
        }
        
        var mesh = builder.Build();

        watch.Stop();
        var build = watch.ElapsedMilliseconds;
        watch.Reset();
        watch.Start();

        if (mesh.vertices.Length != 0) {
            // Use temporary variable to avoid drawing while data is being written off-thread
            var temp_vertices = new VertexBuffer(device, typeof(VertexPositionColorTexture), mesh.vertices.Length, BufferUsage.WriteOnly);
            temp_vertices.SetData(mesh.vertices);

            vertices = temp_vertices;
            primitiveCount = mesh.vertices.Length/2;
        } else {
            vertices = null;
        }

        watch.Stop();
        var upload = watch.ElapsedMilliseconds;

        VoxelClient.Log.Info($"Build: {build}, Upload: {upload}");
    }

    private void GenerateQuad(MeshBuilder builder, ChunkView world, BlockPos pos, int direction) {
        var normal = normals[direction];
        var adjustedPos = pos + normal;
        
        var adjacent = world.GetBlock(adjustedPos);
        if (adjacent.IsSolidBlock)
            return;

        var quadVertices = new VertexPositionColorTexture[4];
        for (var vertex = 0; vertex < 4; vertex++) {
            var coords = (pos + vertexOffsets[direction, vertex]).vector3;
            var tx = new Vector2(textureCoords[direction, vertex, 0], textureCoords[direction, vertex, 1]);
            var aoPos1 = adjustedPos + aoOffsets[direction, vertex, 0];
            var aoPos2 = adjustedPos + aoOffsets[direction, vertex, 1];
            var aoPos3 = aoPos1 + aoOffsets[direction, vertex, 1];
            var ao1 = world.GetBlock(aoPos1).IsSolidBlock ? 1 : 0;
            var ao2 = world.GetBlock(aoPos2).IsSolidBlock ? 1 : 0;
            var ao3 = world.GetBlock(aoPos3).IsSolidBlock ? 1 : 0;
            var color = 1 - AO_STEP * (ao1 + ao2 + ao3);
            quadVertices[vertex] = new(coords, new(color, color, color), tx);
        }
        
        builder.Quad(quadVertices);
    }
}
