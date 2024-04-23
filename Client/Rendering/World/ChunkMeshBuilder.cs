using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using GlmSharp;
using Newtonsoft.Json.Serialization;
using Voxel.Client.Rendering.Models;
using Voxel.Client.Rendering.Utils;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Client.World;
using Voxel.Common.Tile;
using Voxel.Common.Util;
using Voxel.Common.World.Storage;
using Voxel.Common.World.Views;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.World;

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

                //Left Face
                FaceToNeighborIndexes[0] = [
                    OffsetToNeighborIndex(-1, 0, 0),
                    OffsetToNeighborIndex(-1, 0, -1),
                    OffsetToNeighborIndex(-1, -1, -1),
                    OffsetToNeighborIndex(-1, -1, 0),
                    OffsetToNeighborIndex(-1, -1, 1),
                    OffsetToNeighborIndex(-1, 0, 1),
                    OffsetToNeighborIndex(-1, 1, 1),
                    OffsetToNeighborIndex(-1, 1, 0),
                    OffsetToNeighborIndex(-1, 1, -1),
                ];

                //Right Face
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

                //Bottom Face
                FaceToNeighborIndexes[2] = [
                    OffsetToNeighborIndex(0, -1, 0),
                    OffsetToNeighborIndex(-1, -1, 0),
                    OffsetToNeighborIndex(-1, -1, -1),
                    OffsetToNeighborIndex(0, -1, -1),
                    OffsetToNeighborIndex(1, -1, -1),
                    OffsetToNeighborIndex(1, -1, 0),
                    OffsetToNeighborIndex(1, -1, 1),
                    OffsetToNeighborIndex(0, -1, 1),
                    OffsetToNeighborIndex(-1, -1, 1),
                ];
                //Top Face
                FaceToNeighborIndexes[3] = [
                    OffsetToNeighborIndex(0, 1, 0),
                    OffsetToNeighborIndex(0, 1, -1),
                    OffsetToNeighborIndex(-1, 1, -1),
                    OffsetToNeighborIndex(-1, 1, 0),
                    OffsetToNeighborIndex(-1, 1, 1),
                    OffsetToNeighborIndex(0, 1, 1),
                    OffsetToNeighborIndex(1, 1, 1),
                    OffsetToNeighborIndex(1, 1, 0),
                    OffsetToNeighborIndex(1, 1, -1),
                ];

                //Backward Face
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
                //Forward Face
                FaceToNeighborIndexes[5] = [
                    OffsetToNeighborIndex(0, 0, 1),
                    OffsetToNeighborIndex(-1, 0, 1),
                    OffsetToNeighborIndex(-1, -1, 1),
                    OffsetToNeighborIndex(0, -1, 1),
                    OffsetToNeighborIndex(1, -1, 1),
                    OffsetToNeighborIndex(1, 0, 1),
                    OffsetToNeighborIndex(1, 1, 1),
                    OffsetToNeighborIndex(0, 1, 1),
                    OffsetToNeighborIndex(-1, 1, 1),
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

                Block[] neighbors = new Block[NeighborPositions.Length];
                Block[] faceBlocks = new Block[9];
                vec4 AO = vec4.Zero;

                foreach (var pos in Iteration.Cubic(PositionExtensions.ChunkSize)) {
                    var block = centerStorage[baseIndex];

                    if (!BlockModelManager.TryGetModel(block, out var mdl)) {
                        baseIndex++;
                        continue;
                    }

                    // Get neighbors
                    long neighborListIndex = baseIndex++ * NeighborPositions.Length;

                    bool isVisible = false;
                    for (int n = 0; n < NeighborPositions.Length; n++) {
                        var checkTuple = NeighborIndexes[neighborListIndex + n];
                        var checkBlock = ChunkStorages[checkTuple.Item1][checkTuple.Item2];

                        //Mark if any side of this block is visible.
                        isVisible |= checkBlock.IsAir;
                        neighbors[n] = checkBlock;
                    }

                    //Foreach face...
                    for (int face = 0; face < 6; face++) {

                        //Get all the blocks to check for this face into a list.
                        var fIndexList = FaceToNeighborIndexes[face];
                        for (var fb = 0; fb < faceBlocks.Length; fb++)
                            faceBlocks[fb] = neighbors[fIndexList[fb]];

                        //If block directly on face is solid, skip face.
                        if (!faceBlocks[0].IsAir)
                            continue;

                        float calculateAO(float s1, float corner, float s2) {
                            if (s1 == 1 && s2 == 1)
                                return 3;
                            return s1 + s2 + corner;
                        }

                        AO = vec4.Zero;

                        //if (face <= 1) {
                        AO[0] = calculateAO(faceBlocks[1].Settings.Solidity, faceBlocks[2].Settings.Solidity, faceBlocks[3].Settings.Solidity);
                        AO[1] = calculateAO(faceBlocks[3].Settings.Solidity, faceBlocks[4].Settings.Solidity, faceBlocks[5].Settings.Solidity);
                        AO[2] = calculateAO(faceBlocks[5].Settings.Solidity, faceBlocks[6].Settings.Solidity, faceBlocks[7].Settings.Solidity);
                        AO[3] = calculateAO(faceBlocks[7].Settings.Solidity, faceBlocks[8].Settings.Solidity, faceBlocks[1].Settings.Solidity);
                        //}


                        AddVertices(pos, mdl.SidedVertices[face], AO);
                    }

                    //If all sides are hidden, don't add center vertices.
                    if (isVisible)
                        AddVertices(pos, mdl.SidedVertices[6].AsSpan(), vec4.Zero);
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

        private void AddVertices(vec3 centerPos, Span<TerrainVertex> span, vec4 ao) {
            for (int i = 0; i < span.Length; i++) {
                var vtx = span[i];
                vtx.position += centerPos;
                VertexCache.Vertex(TerrainVertex.Pack(vtx, ao));
            }
        }
    }
}
