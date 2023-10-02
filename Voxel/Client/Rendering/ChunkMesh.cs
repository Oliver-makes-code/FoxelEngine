using System.Collections.Generic;
using System.Diagnostics;
using GlmSharp;
using Microsoft.Xna.Framework.Graphics;
using Voxel.Client.World;
using Voxel.Common;
using Voxel.Common.World;

namespace Voxel.Client.Rendering;

public class ChunkMesh {
    public const float AO_STEP = 0.1f;
    public const uint CHUNK_SIZE = 32;
    public const float TEXTIRE_START = 0f;
    public const float TEXTURE_SIZE = 16f;
    
    private static readonly TilePos[] normals = { TilePos.East, TilePos.West, TilePos.Up, TilePos.Down, TilePos.South, TilePos.North };
    private static readonly TilePos[,] vertexOffsets = {
        { TilePos.East + TilePos.South, TilePos.East + TilePos.Up + TilePos.South, TilePos.East + TilePos.Up, TilePos.East }, // East
        { TilePos.Origin, TilePos.Up, TilePos.Up + TilePos.South, TilePos.South }, // West
        { TilePos.Up, TilePos.East + TilePos.Up, TilePos.East + TilePos.Up + TilePos.South, TilePos.Up + TilePos.South }, // Up
        { TilePos.South, TilePos.East + TilePos.South, TilePos.East, TilePos.Origin }, // Down
        { TilePos.South, TilePos.Up + TilePos.South, TilePos.East + TilePos.Up + TilePos.South, TilePos.East + TilePos.South }, // South
        { TilePos.Origin, TilePos.East, TilePos.East + TilePos.Up, TilePos.Up } // North
    };
    private static readonly float[,,] textureCoords = {
        { { TEXTIRE_START, TEXTURE_SIZE }, { TEXTIRE_START, TEXTIRE_START }, { TEXTURE_SIZE, TEXTIRE_START }, { TEXTURE_SIZE, TEXTURE_SIZE } }, // East
        { { TEXTIRE_START, TEXTURE_SIZE }, { TEXTIRE_START, TEXTIRE_START }, { TEXTURE_SIZE, TEXTIRE_START }, { TEXTURE_SIZE, TEXTURE_SIZE } }, // West
        { { TEXTURE_SIZE, TEXTURE_SIZE }, { TEXTIRE_START, TEXTURE_SIZE }, { TEXTIRE_START, TEXTIRE_START }, { TEXTURE_SIZE, TEXTIRE_START } }, // Up
        { { TEXTIRE_START, TEXTIRE_START }, { TEXTURE_SIZE, TEXTIRE_START }, { TEXTURE_SIZE, TEXTURE_SIZE }, { TEXTIRE_START, TEXTURE_SIZE } }, // Down
        { { TEXTIRE_START, TEXTURE_SIZE }, { TEXTIRE_START, TEXTIRE_START }, { TEXTURE_SIZE, TEXTIRE_START }, { TEXTURE_SIZE, TEXTURE_SIZE } }, // South
        { { TEXTURE_SIZE, TEXTURE_SIZE }, { TEXTIRE_START, TEXTURE_SIZE }, { TEXTIRE_START, TEXTIRE_START }, { TEXTURE_SIZE, TEXTIRE_START } }  // North
    };
    private static readonly TilePos[,,] aoOffsets = {
        { { TilePos.Down, TilePos.South }, { TilePos.Up, TilePos.South }, { TilePos.Up, TilePos.North }, { TilePos.Down, TilePos.North } }, // East
        { { TilePos.Down, TilePos.North }, { TilePos.Up, TilePos.North }, { TilePos.Up, TilePos.South }, { TilePos.Down, TilePos.South } }, // West
        { { TilePos.West, TilePos.North }, { TilePos.East, TilePos.North }, { TilePos.East, TilePos.South }, { TilePos.West, TilePos.South } }, // Up
        { { TilePos.West, TilePos.South }, { TilePos.East, TilePos.South }, { TilePos.East, TilePos.North }, { TilePos.West, TilePos.North } }, // Down
        { { TilePos.West, TilePos.Down }, { TilePos.West, TilePos.Up }, { TilePos.East, TilePos.Up }, { TilePos.East, TilePos.Down } }, // South
        { { TilePos.West, TilePos.Down }, { TilePos.East, TilePos.Down }, { TilePos.East, TilePos.Up }, { TilePos.West, TilePos.Up } }  // North
    };

    private static List<long> buildAvg = new();
    private static long buildMax = 0;
    private static long buildMin = long.MaxValue;
    private static List<long> uploadAvg = new();
    private static long uploadMax = 0;
    private static long uploadMin = long.MaxValue;
    private static VertexPositionColorTexture[][] quadVertices = {
        new VertexPositionColorTexture[4],
        new VertexPositionColorTexture[4],
        new VertexPositionColorTexture[4]
    };
    
    public int primitiveCount;
    VertexBuffer? vertices;

    public static void SetupThreadCount(int threadCount) {
        quadVertices = new VertexPositionColorTexture[threadCount][];
        for (int i = 0; i < threadCount; i++) {
            quadVertices[i] = new VertexPositionColorTexture[4];
        }
    }

    public void Draw(GraphicsDevice device, Effect effect) {
        if (vertices == null)
            return;

        device.SetVertexBuffer(vertices);

        effect.CurrentTechnique.Passes[0].Apply();

        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
    }

    public void BuildChunk(GraphicsDevice device, ClientWorld world, ChunkPos pos, int threadNumber) {
        var watch = new Stopwatch();
        watch.Start();
        MeshBuilder builder = new(threadNumber);
        ChunkView view = new(world.world, pos);
        
        for (var x = 0; x < CHUNK_SIZE; x++) {
            for (var y = 0; y < CHUNK_SIZE; y++) {
                for (var z = 0; z < CHUNK_SIZE; z++) {
                    ChunkTilePos chunkPos = new(false, x, y, z);
                    var blockPos = new TilePos(ref pos, ref chunkPos);
                    var block = view.GetBlock(blockPos);
        
                    if (!block.IsSolidBlock) {
                        continue;
                    }
        
                    for (var direction = 0; direction < 6; direction++) {
                        var normal = normals[direction];
        
                        var adjacent = view.GetBlock(blockPos + normal);
                        if (adjacent.IsSolidBlock)
                            continue;
                        GenerateQuad(view, blockPos, direction, threadNumber);
                        builder.Quad(quadVertices[threadNumber]);
                    }
                }
            }
        }
        
        builder.Build();

        if (builder.idx != 0) {
            // Use temporary variable to avoid drawing while data is being written off-thread
            var tempVertices = new VertexBuffer(device, typeof(VertexPositionColorTexture), builder.idx*4, BufferUsage.WriteOnly);
            tempVertices.SetData(Mesh.vertices[threadNumber], 0, builder.idx*4);

            vertices = tempVertices;
            primitiveCount = builder.idx*2;
        } else {
            vertices = null;
        }
    }

    private static void GenerateQuad(ChunkView world, TilePos pos, int direction, int threadNumber) {
        var normal = normals[direction];
        var adjustedPos = pos + normal;
        
        for (var vertex = 0; vertex < 4; vertex++) {
            var coords = (pos + vertexOffsets[direction, vertex]).vector3;
            var tx = new vec2(textureCoords[direction, vertex, 0], textureCoords[direction, vertex, 1]);
            var aoPos1 = adjustedPos + aoOffsets[direction, vertex, 0];
            var aoPos2 = adjustedPos + aoOffsets[direction, vertex, 1];
            var aoPos3 = aoPos1 + aoOffsets[direction, vertex, 1];
            var ao1 = world.GetBlock(aoPos1).IsSolidBlock ? 1 : 0;
            var ao2 = world.GetBlock(aoPos2).IsSolidBlock ? 1 : 0;
            var ao3 = world.GetBlock(aoPos3).IsSolidBlock ? 1 : 0;
            var color = 1 - AO_STEP * (ao1 + ao2 + ao3);
            quadVertices[threadNumber][vertex] = new(coords.ToXnaVector3(), new(color, color, color), tx.ToXnaVector2());
        }
    }
}
