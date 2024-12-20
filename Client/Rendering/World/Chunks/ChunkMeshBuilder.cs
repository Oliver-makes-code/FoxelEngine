using System;
using System.Threading;
using GlmSharp;
using Foxel.Client.Rendering.Models;
using Foxel.Client.Rendering.VertexTypes;
using Foxel.Client.World;
using Foxel.Common.Util;
using Foxel.Common.World.Storage;
using Foxel.Core.Rendering;
using Foxel.Common.World.Content.Blocks;
using Foxel.Common.World.Content.Blocks.State;

namespace Foxel.Client.Rendering.World.Chunks;

public static class ChunkMeshBuilder {
    public static int count => meshingJobs.Length;

    private static ChunkMeshJob[] meshingJobs = [];

    public static void Init(int threadCount) {
        foreach (var job in meshingJobs)
            job.Stop();

        meshingJobs = new ChunkMeshJob[threadCount];

        for (var i = 0; i < meshingJobs.Length; i++)
            meshingJobs[i] = new();
    }

    public static bool Rebuild(ChunkRenderSlot slot, ivec3 chunkPosition) {
        foreach (var job in meshingJobs) {
            if (job.Build(slot, chunkPosition))
                return true;
        }

        return false;
    }

    /*public static void RebuildImmediate() {

    }*/

    public static void Stop() {
        foreach (var job in meshingJobs)
            job.Stop();
    }

    public static bool IsActive(int thread) {
        if (thread >= count)
            return false;
        return meshingJobs[thread].isBuilding;
    }

    private class ChunkMeshJob {

        private static readonly ivec3[] NeighborPositions = new ivec3[26];
        private static readonly ivec3[] DiagonalSelfNeighborPositions;
        private static readonly (ushort, ushort)[] NeighborIndexes;

        private static readonly ushort[][] FaceToNeighborIndexes;

        public bool isBuilding;

        private readonly Thread Thread;
        private readonly VertexConsumer<TerrainVertex.Packed> VertexCache = new();

        private bool isStopped = false;

        private ChunkRenderSlot? target;
        private ivec3 position;
        private readonly ChunkStorage[] ChunkStorages = new ChunkStorage[DiagonalSelfNeighborPositions.Length];

        static ChunkMeshJob() {
            {
                int nIndex = 0;
                foreach (var nPos in Iteration.Cubic(-1, 2)) {
                    var modified = nPos;

                    if (modified == ivec3.Zero)
                        continue;

                    NeighborPositions[nIndex++] = modified;
                }

                DiagonalSelfNeighborPositions = new ivec3[27];
                int id = 0;
                foreach (var pos in Iteration.Cubic(-1, 2))
                    DiagonalSelfNeighborPositions[id++] = pos;
            }

            //Neighbor blocks
            {
                NeighborIndexes = new (ushort, ushort)[NeighborPositions.Length * PositionExtensions.ChunkCapacity];
                uint baseIndex = 0;

                foreach (var centerPos in Iteration.Cubic(PositionExtensions.ChunkSize)) {

                    for (int i = 0; i < NeighborPositions.Length; i++) {
                        var nPos = centerPos + NeighborPositions[i];

                        //Index of the entry we want to check.
                        ushort targetChunk = 13; //13 is the 'center' chunk in a 3x3x3

                        if (nPos.z < 0) {
                            nPos.z = nPos.z.Loop(PositionExtensions.ChunkSize);
                            targetChunk -= 1;
                        } else if (nPos.z >= PositionExtensions.ChunkSize) {
                            nPos.z = nPos.z.Loop(PositionExtensions.ChunkSize);
                            targetChunk += 1;
                        }

                        if (nPos.y < 0) {
                            nPos.y = nPos.y.Loop(PositionExtensions.ChunkSize);
                            targetChunk -= 3;
                        } else if (nPos.y >= PositionExtensions.ChunkSize) {
                            nPos.y = nPos.y.Loop(PositionExtensions.ChunkSize);
                            targetChunk += 3;
                        }


                        if (nPos.x < 0) {
                            nPos.x = nPos.x.Loop(PositionExtensions.ChunkSize);
                            targetChunk -= 9;
                        } else if (nPos.x >= PositionExtensions.ChunkSize) {
                            nPos.x = nPos.x.Loop(PositionExtensions.ChunkSize);
                            targetChunk += 9;
                        }

                        ushort targetIndex = (ushort)(nPos.z + (nPos.y * PositionExtensions.ChunkSize) + (nPos.x * PositionExtensions.ChunkStep));
                        NeighborIndexes[(baseIndex * NeighborPositions.Length) + i] = (targetChunk, targetIndex);
                    }

                    baseIndex++;
                }
            }

            {
                FaceToNeighborIndexes = new ushort[6][];

                // West Face
                FaceToNeighborIndexes[0] = [
                    OffsetToNeighborIndex(-1, 0, 0),
                    OffsetToNeighborIndex(-1, -1, 0),
                    OffsetToNeighborIndex(-1, -1, 1),
                    OffsetToNeighborIndex(-1, 0, 1),
                    OffsetToNeighborIndex(-1, 1, 1),
                    OffsetToNeighborIndex(-1, 1, 0),
                    OffsetToNeighborIndex(-1, 1, -1),
                    OffsetToNeighborIndex(-1, 0, -1),
                    OffsetToNeighborIndex(-1, -1, -1),
                ];

                // East Face
                FaceToNeighborIndexes[1] = [
                    OffsetToNeighborIndex(1, 0, 0),
                    OffsetToNeighborIndex(1, -1, 0),
                    OffsetToNeighborIndex(1, -1, -1),
                    OffsetToNeighborIndex(1, 0, -1),
                    OffsetToNeighborIndex(1, 1, -1),
                    OffsetToNeighborIndex(1, 1, 0),
                    OffsetToNeighborIndex(1, 1, 1),
                    OffsetToNeighborIndex(1, 0, 1),
                    OffsetToNeighborIndex(1, -1, 1),
                ];

                // Down Face
                FaceToNeighborIndexes[2] = [
                    OffsetToNeighborIndex(0, -1, 0),
                    OffsetToNeighborIndex(1, -1, 0),
                    OffsetToNeighborIndex(1, -1, 1),
                    OffsetToNeighborIndex(0, -1, 1),
                    OffsetToNeighborIndex(-1, -1, 1),
                    OffsetToNeighborIndex(-1, -1, 0),
                    OffsetToNeighborIndex(-1, -1, -1),
                    OffsetToNeighborIndex(0, -1, -1),
                    OffsetToNeighborIndex(1, -1, -1),
                ];
                // Up Face
                FaceToNeighborIndexes[3] = [
                    OffsetToNeighborIndex(0, 1, 0),
                    OffsetToNeighborIndex(1, 1, 0),
                    OffsetToNeighborIndex(1, 1, -1),
                    OffsetToNeighborIndex(0, 1, -1),
                    OffsetToNeighborIndex(-1, 1, -1),
                    OffsetToNeighborIndex(-1, 1, 0),
                    OffsetToNeighborIndex(-1, 1, 1),
                    OffsetToNeighborIndex(0, 1, 1),
                    OffsetToNeighborIndex(1, 1, 1),
                ];

                // North Face
                FaceToNeighborIndexes[4] = [
                    OffsetToNeighborIndex(0, 0, -1),
                    OffsetToNeighborIndex(0, -1, -1),
                    OffsetToNeighborIndex(-1, -1, -1),
                    OffsetToNeighborIndex(-1, 0, -1),
                    OffsetToNeighborIndex(-1, 1, -1),
                    OffsetToNeighborIndex(0, 1, -1),
                    OffsetToNeighborIndex(1, 1, -1),
                    OffsetToNeighborIndex(1, 0, -1),
                    OffsetToNeighborIndex(1, -1, -1),
                ];
                // South Face
                FaceToNeighborIndexes[5] = [
                    OffsetToNeighborIndex(0, 0, 1),
                    OffsetToNeighborIndex(0, -1, 1),
                    OffsetToNeighborIndex(1, -1, 1),
                    OffsetToNeighborIndex(1, 0, 1),
                    OffsetToNeighborIndex(1, 1, 1),
                    OffsetToNeighborIndex(0, 1, 1),
                    OffsetToNeighborIndex(-1, 1, 1),
                    OffsetToNeighborIndex(-1, 0, 1),
                    OffsetToNeighborIndex(-1, -1, 1),
                ];
            }
        }

        public ChunkMeshJob() {
            Thread = new(WorkLoop) {
                IsBackground = true
            };

            Thread.Start();
        }

        private static ushort OffsetToNeighborIndex(int x, int y, int z) {
            ushort val = (ushort)(((x + 1) * 9) + ((y + 1) * 3) + (z + 1));

            if (val >= 14)
                val--;
            return val;
        }

        public bool Build(ChunkRenderSlot target, ivec3 worldPosition) {
            if (isBuilding)
                return false;

            this.target = target;
            position = worldPosition;

            //Simply ignore empty chunks.
            if (target.targetChunk!.isEmpty)
                return true;

            //Copy snapshot of current adjacent chunk storage to a cache.
            for (int i = 0; i < DiagonalSelfNeighborPositions.Length; i++) {
                var pos = DiagonalSelfNeighborPositions[i] + target.targetChunk!.ChunkPosition;

                if (!target.targetChunk!.World.TryGetChunkRaw(pos, out var c) || c is not ClientChunk clientChunk || !clientChunk.isFilled)
                    return false;
            }


            //TODO: Check if chunks exist BEFORE copying.
            for (int i = 0; i < DiagonalSelfNeighborPositions.Length; i++) {
                var pos = DiagonalSelfNeighborPositions[i] + target.targetChunk!.ChunkPosition;

                if (target.targetChunk!.World.TryGetChunkRaw(pos, out var c))
                    ChunkStorages[i] = c.CopyStorage();
            }

            isBuilding = true;
            return true;
        }

        public void Stop() {
            if (isStopped)
                return;
            
            isStopped = true;

            //Wait for thread to finish.
            Thread.Join();
        }

        private void WorkLoop() {
            while (!isStopped) {
                if (!isBuilding) {
                    Thread.Sleep(15);
                    continue;
                }
                VertexCache.Clear();

                uint baseIndex = 0;

                var centerStorage = ChunkStorages[13];

                BlockState[] neighbors = new BlockState[NeighborPositions.Length];
                BlockState[] faceBlocks = new BlockState[9];

                foreach (var pos in Iteration.Cubic(PositionExtensions.ChunkSize)) {
                    var state = centerStorage[baseIndex];
                    var block = state.Block;

                    if (!BlockModelManager.TryGetModel(block, out var mdl)) {
                        baseIndex++;
                        continue;
                    }

                    // Get neighbors
                    long neighborListIndex = baseIndex++ * NeighborPositions.Length;

                    bool isVisible = false;
                    for (int n = 0; n < NeighborPositions.Length; n++) {
                        var checkTuple = NeighborIndexes[neighborListIndex + n];
                        var checkState = ChunkStorages[checkTuple.Item1][checkTuple.Item2];
                        var checkBlock = checkState.Block;

                        neighbors[n] = checkState;

                        //Mark if any side of this block is visible.
                        isVisible |= checkState.Settings.IgnoresCollision;
                        isVisible |= !checkBlock.GetShape(checkState).SideFullSquare(((Face)n).Opposite());
                    }

                    //Foreach face...
                    for (int face = 0; face < 6; face++) {

                        //Get all the blocks to check for this face into a list.
                        var fIndexList = FaceToNeighborIndexes[face];
                        for (var fb = 0; fb < faceBlocks.Length; fb++)
                            faceBlocks[fb] = neighbors[fIndexList[fb]];

                        var faceState = faceBlocks[0];

                        //If block directly on face is solid, skip face.
                        if (
                            !faceState.Settings.IgnoresCollision 
                            && faceState.Block.GetShape(faceState).SideFullSquare(((Face)face).Opposite())
                        )
                            continue;

                        AddVertices(pos, mdl.SidedVertices[face]);
                    }

                    //If all sides are hidden, don't add center vertices.
                    if (isVisible)
                        AddVertices(pos, mdl.SidedVertices[6].AsSpan());
                }

                uint indexCount = (uint)VertexCache.Count / 4 * 6;
                if (indexCount != 0) {
                    var mesh = new ChunkRenderSlot.ChunkMesh(
                        target!.Client,
                        VertexCache.AsSpan(), indexCount,
                        position
                    );

                    target.SetMesh(mesh);
                }
                
                foreach (var storage in ChunkStorages)
                    storage.Dispose();
                
                isBuilding = false;
            }
        }

        private void AddVertices(vec3 centerPos, Span<TerrainVertex> span) {
            for (int i = 0; i < span.Length; i++) {
                var vtx = span[i];
                vtx.position += centerPos;
                VertexCache.Vertex(TerrainVertex.Pack(vtx));
            }
        }
    }
}
