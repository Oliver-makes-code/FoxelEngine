using System;
using System.Threading;
using GlmSharp;
using Newtonsoft.Json.Serialization;
using Voxel.Client.Rendering.Models;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Common.Tile;
using Voxel.Common.Util;
using Voxel.Common.World.Storage;
using Voxel.Common.World.Views;

namespace Voxel.Client.Rendering.World;

public static class ChunkMeshBuilder {

    private static readonly ivec3[] ChunkNeighbors = new ivec3[27];

    private static ChunkMeshJob[] meshingJobs = Array.Empty<ChunkMeshJob>();

    public static void Init(int threadCount) {
        foreach (var job in meshingJobs)
            job.Stop();

        meshingJobs = new ChunkMeshJob[threadCount];

        for (var i = 0; i < meshingJobs.Length; i++)
            meshingJobs[i] = new ChunkMeshJob();
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

    private class ChunkMeshJob {

        private static readonly ivec3[] NeighborPositions = {
            new(-1, 0, 0), new(1, 0, 0),
            new(0, -1, 0), new(0, 1, 0),
            new(0, 0, -1), new(0, 0, 1),
        };
        private static readonly ivec3[] DiagonalSelfNeighborPositions;
        private static readonly (ushort, uint)[] NeighborIndexes;

        private readonly Thread Thread;
        private readonly BasicVertex.Packed[] VertexCache = new BasicVertex.Packed[PositionExtensions.ChunkCapacity * 4 * 6 * 8];

        private bool isStopped = false;

        public bool isBuilding;

        private uint vertexIndex;

        private ChunkRenderSlot target;
        private ivec3 position;
        private readonly ChunkStorage[] chunkStorages = new ChunkStorage[27];

        static ChunkMeshJob() {
            {
                DiagonalSelfNeighborPositions = new ivec3[27];
                int id = 0;

                foreach (var pos in Iteration.Cubic(-1, 2))
                    DiagonalSelfNeighborPositions[id++] = pos;
            }

            {
                NeighborIndexes = new (ushort, uint)[NeighborPositions.Length * PositionExtensions.ChunkCapacity];

                uint baseIndex = 0;

                foreach (var centerPos in Iteration.Cubic(PositionExtensions.ChunkSize)) {

                    for (int i = 0; i < NeighborPositions.Length; i++) {
                        var nPos = centerPos + NeighborPositions[i];

                        //Index of the entry we want to check.
                        ushort targetChunk = 13; //13 is the 'center' chunk in a 3x3x3

                        if (nPos.z < 0) {
                            nPos.z = nPos.z.Loop(PositionExtensions.ChunkSize);
                            targetChunk -= 1;
                        }
                        else if (nPos.z >= PositionExtensions.ChunkSize) {
                            nPos.z = nPos.z.Loop(PositionExtensions.ChunkSize);
                            targetChunk += 1;
                        }

                        if (nPos.y < 0) {
                            nPos.y = nPos.y.Loop(PositionExtensions.ChunkSize);
                            targetChunk -= 3;
                        }
                        else if (nPos.y >= PositionExtensions.ChunkSize) {
                            nPos.y = nPos.y.Loop(PositionExtensions.ChunkSize);
                            targetChunk += 3;
                        }


                        if (nPos.x < 0) {
                            nPos.x = nPos.x.Loop(PositionExtensions.ChunkSize);
                            targetChunk -= 9;
                        }
                        else if (nPos.x >= PositionExtensions.ChunkSize) {
                            nPos.x = nPos.x.Loop(PositionExtensions.ChunkSize);
                            targetChunk += 9;
                        }

                        uint targetIndex = (uint)(nPos.z + (nPos.y * PositionExtensions.ChunkSize) + (nPos.x * PositionExtensions.ChunkStep));
                        NeighborIndexes[(baseIndex * NeighborPositions.Length) + i] = (targetChunk, targetIndex);
                    }

                    baseIndex++;
                }
            }
        }

        public ChunkMeshJob() {
            Thread = new(WorkLoop);
            Thread.IsBackground = true;

            Thread.Start();
        }

        public bool Build(ChunkRenderSlot target, ivec3 worldPosition) {
            if (isBuilding)
                return false;

            this.target = target;
            position = worldPosition;

            //Simply ignore empty chunks.
            if (target.TargetChunk!.IsEmpty)
                return true;

            //Copy snapshot of current adjacent chunk storage to a cache.
            for (int i = 0; i < DiagonalSelfNeighborPositions.Length; i++) {
                var pos = DiagonalSelfNeighborPositions[i] + target.TargetChunk!.ChunkPosition;

                if (!target.TargetChunk!.World.IsChunkLoadedRaw(pos))
                    return false;
            }


            //TODO - Check if chunks exist BEFORE copying.
            for (int i = 0; i < DiagonalSelfNeighborPositions.Length; i++) {
                var pos = DiagonalSelfNeighborPositions[i] + target.TargetChunk!.ChunkPosition;

                if (target.TargetChunk!.World.TryGetChunkRaw(pos, out var c))
                    chunkStorages[i] = c.CopyStorage();
            }

            isBuilding = true;
            return true;
        }

        private void WorkLoop() {
            while (!isStopped) {
                if (!isBuilding) {
                    Thread.Sleep(15);
                    continue;
                }
                vertexIndex = 0;

                //try {
                uint baseIndex = 0;

                var centerStorage = chunkStorages[13];

                Console.Out.WriteLine("Building chunk...");
                var start = DateTime.Now;

                foreach (var pos in Iteration.Cubic(PositionExtensions.ChunkSize)) {
                    var block = centerStorage[baseIndex];

                    //Skip air blocks...
                    if (block == Blocks.Air) {
                        baseIndex++;
                        continue;
                    }

                    //TODO - Replace with actual model system
                    var mdl = BlockModel.Default;

                    var neighborListIndex = (baseIndex++) * 6;

                    bool allNotVisible = true;

                    for (int n = 0; n < 6; n++) {
                        //var checkTuple = NeighborIndexes[neighborListIndex + n];
                        //var checkBlock = chunkStorages[checkTuple.Item1][checkTuple.Item2];

                        //If block isn't air, it's blocked.
                        //if (checkBlock != Blocks.Air) continue;
                        //Tag this block as being visible anywhere.
                        allNotVisible = false;

                        //Add that side's vertices.
                        AddVertices(pos, mdl.SidedVertices[n]);
                    }

                    //If all sides are hidden, don't add center vertices.
                    if (!allNotVisible)
                        AddVertices(pos, mdl.SidedVertices[6].AsSpan());
                }

                uint indexCount = (vertexIndex / 4) * 6;
                if (indexCount != 0) {
                    var mesh = new ChunkRenderSlot.ChunkMesh(
                        target.Client,
                        VertexCache.AsSpan(0, (int)vertexIndex), indexCount,
                        position
                    );

                    target.SetMesh(mesh);
                }

                //} catch (Exception e) {
                //Console.Out.WriteLine(e);
                //throw;
                //}

                //Dispose of storage objects now that we're done with them.
                foreach (var storage in chunkStorages)
                    storage.Dispose();

                var end = DateTime.Now;

                var totalMs = (end - start).TotalMilliseconds;

                Console.Out.WriteLine($"Done Building, took {totalMs:##.#}ms");
                isBuilding = false;
            }
        }

        private void AddVertices(vec3 centerPos, Span<BasicVertex> span) {
            for (int i = 0; i < span.Length; i++) {
                var cp = span[i];
                cp.position += centerPos;
                VertexCache[vertexIndex++] = cp;
            }
        }

        public void Stop() {
            if (isStopped)
                return;

            //Wait for thread to finish.
            Thread.Join();
        }
    }
}
