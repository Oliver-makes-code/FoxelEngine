using System;
using System.Threading;
using GlmSharp;
using Voxel.Client.Rendering.Models;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Common.Tile;
using Voxel.Common.Util;
using Voxel.Common.World.Storage;
using Voxel.Common.World.Views;

namespace Voxel.Client.Rendering.World;

public static class ChunkMeshBuilder {

    private static readonly ivec3[] _chunkNeighbors;

    private static ChunkMeshJob[] _meshingJobs = Array.Empty<ChunkMeshJob>();

    static ChunkMeshBuilder() {
        _chunkNeighbors = new ivec3[27];
    }

    public static void Init(int threadCount) {
        foreach (var job in _meshingJobs)
            job.Stop();

        _meshingJobs = new ChunkMeshJob[threadCount];

        for (var i = 0; i < _meshingJobs.Length; i++)
            _meshingJobs[i] = new ChunkMeshJob();
    }

    public static bool Rebuild(ChunkRenderSlot slot) {

        foreach (var job in _meshingJobs) {
            if (job.Build(slot))
                return true;
        }

        return false;
    }

    /*public static void RebuildImmediate() {

    }*/

    public static void Stop() {
        foreach (var job in _meshingJobs)
            job.Stop();
    }

    private class ChunkMeshJob {

        private static readonly ivec3[] neighborPositions = new ivec3[] {
            new ivec3(-1, 0, 0), new ivec3(1, 0, 0),
            new ivec3(0, -1, 0), new ivec3(0, 1, 0),
            new ivec3(0, 0, -1), new ivec3(0, 0, 1),
        };
        private static readonly ivec3[] diagonalSelfNeighborPositions;
        private static readonly (ushort, uint)[] _neighborIndexes;

        private bool _isStopped = false;
        private readonly Thread _thread;

        public bool IsBuilding;

        private BasicVertex.Packed[] _vertexCache = new BasicVertex.Packed[PositionExtensions.CHUNK_CAPACITY * 4 * 6 * 8];
        private uint _vertexIndex = 0;

        private ChunkRenderSlot _target;
        private readonly ChunkStorage[] _chunkStorages = new ChunkStorage[27];

        static ChunkMeshJob() {

            {
                diagonalSelfNeighborPositions = new ivec3[27];
                var id = 0;

                for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                for (int z = -1; z <= 1; z++)
                    diagonalSelfNeighborPositions[id++] = new ivec3(x, y, z);
            }


            {
                _neighborIndexes = new (ushort, uint)[neighborPositions.Length * PositionExtensions.CHUNK_CAPACITY];

                uint _baseIndex = 0;

                for (uint x = 0; x < PositionExtensions.CHUNK_SIZE; x++)
                for (uint y = 0; y < PositionExtensions.CHUNK_SIZE; y++)
                for (uint z = 0; z < PositionExtensions.CHUNK_SIZE; z++) {
                    ivec3 centerPos = new ivec3((int)x, (int)y, (int)z);

                    for (var i = 0; i < neighborPositions.Length; i++) {
                        var nPos = centerPos + neighborPositions[i];

                        //Index of the entry we want to check.
                        ushort targetChunk = 13; //13 is the 'center' chunk in a 3x3x3

                        if (nPos.z < 0) {
                            nPos.z = nPos.z.Loop(PositionExtensions.CHUNK_SIZE);
                            targetChunk -= 1;
                        }
                        else if (nPos.z >= PositionExtensions.CHUNK_SIZE) {
                            nPos.z = nPos.z.Loop(PositionExtensions.CHUNK_SIZE);
                            targetChunk += 1;
                        }

                        if (nPos.y < 0) {
                            nPos.y = nPos.y.Loop(PositionExtensions.CHUNK_SIZE);
                            targetChunk -= 3;
                        }
                        else if (nPos.z >= PositionExtensions.CHUNK_SIZE) {
                            nPos.y = nPos.y.Loop(PositionExtensions.CHUNK_SIZE);
                            targetChunk += 3;
                        }


                        if (nPos.x < 0) {
                            nPos.x = nPos.x.Loop(PositionExtensions.CHUNK_SIZE);
                            targetChunk -= 9;
                        }
                        else if (nPos.x >= PositionExtensions.CHUNK_SIZE) {
                            nPos.x = nPos.x.Loop(PositionExtensions.CHUNK_SIZE);
                            targetChunk += 9;
                        }

                        uint targetIndex = (uint)(nPos.z + (nPos.y * PositionExtensions.CHUNK_SIZE) + (nPos.x * PositionExtensions.CHUNK_STEP));

                        _neighborIndexes[(_baseIndex * neighborPositions.Length) + i] = (targetChunk, targetIndex);
                    }

                    _baseIndex++;
                }
            }
        }

        public ChunkMeshJob() {
            _thread = new(WorkLoop);
            _thread.IsBackground = true;
        }

        public bool Build(ChunkRenderSlot target) {
            if (IsBuilding)
                return false;

            _target = target;

            //Copy snapshot of current adjacent chunk storage to a cache.
            for (var i = 0; i < diagonalSelfNeighborPositions.Length; i++) {
                var pos = diagonalSelfNeighborPositions[i] + target.TargetChunk!.ChunkPosition;

                if (target.TargetChunk!.World.TryGetChunkRaw(pos, out var c))
                    _chunkStorages[i] = c.CopyStorage();
                else
                    _chunkStorages[i] = new SingleStorage(Blocks.Air, null);
            }

            IsBuilding = true;
            return true;
        }

        private void WorkLoop() {
            while (!_isStopped) {
                if (!IsBuilding) {
                    Thread.Sleep(15);
                    continue;
                }
                _vertexIndex = 0;

                try {

                    uint baseIndex = 0;

                    var centerStorage = _chunkStorages[13];

                    for (uint x = 0; x < PositionExtensions.CHUNK_SIZE; x++)
                    for (uint y = 0; y < PositionExtensions.CHUNK_SIZE; y++)
                    for (uint z = 0; z < PositionExtensions.CHUNK_SIZE; z++) {
                        var block = centerStorage[baseIndex];

                        //Skip air blocks...
                        if (block == Blocks.Air) continue;
                        //TODO - Replace with actual model system
                        var mdl = BlockModel.DEFAULT;

                        var neighborListIndex = baseIndex * 6;

                        bool allNotVisible = true;

                        for (int n = 0; n < 6; n++) {
                            var checkTuple = _neighborIndexes[neighborListIndex + n];
                            var checkBlock = _chunkStorages[checkTuple.Item1][checkTuple.Item2];

                            //If block isn't air, it's blocked.
                            if (checkBlock != Blocks.Air) continue;
                            //Tag this block as being visible anywhere.
                            allNotVisible = false;

                            //Add that side's vertices.
                            AddVertices(mdl.SidedVertices[n]);
                        }

                        //If all sides are hidden, don't add center vertices.
                        if (!allNotVisible)
                            AddVertices(mdl.SidedVertices[6].AsSpan());
                    }

                    var indexCount = (_vertexIndex / 4) * 6;
                    if (indexCount != 0) {
                        var mesh = new ChunkRenderSlot.ChunkMesh(_target.RenderSystem);

                        mesh.SetBuffer(_vertexCache.AsSpan(0, (int)_vertexIndex), indexCount);

                        _target.SetMesh(mesh);
                    }

                } catch (Exception e) {
                    Console.Out.WriteLine(e);
                }


                IsBuilding = false;
            }
        }

        private void AddVertices(Span<BasicVertex> span) {
            for (int i = 0; i < span.Length; i++)
                _vertexCache[_vertexIndex++] = span[i];
        }

        private void AddVertex(vec3 pos, vec4 color) {
            _vertexCache[_vertexIndex++] = new BasicVertex { Position = pos, Color = color };
        }

        public void Stop() {
            if (_isStopped)
                return;

            //Wait for thread to finish.
            _thread.Join();
        }
    }
}
