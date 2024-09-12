using System;
using GlmSharp;
using Veldrid;
using Foxel.Client.Rendering.Debug;
using Foxel.Client.Rendering.VertexTypes;
using Foxel.Common.Collision;
using Foxel.Common.Util;
using Foxel.Common.World;
using Foxel.Core.Rendering;
using Foxel.Core.Rendering.Resources.Buffer;

namespace Foxel.Client.Rendering.World.Chunks;

/// <summary>
/// Holds the mesh data for a single chunk, its render state, etc, etc.
/// </summary>
public class ChunkRenderSlot : Renderer {

    public uint? lastVersion;
    public Chunk? targetChunk { get; private set; }

    public ivec3 RealPosition { get; private set; } = ivec3.MinValue;

    private readonly object MeshLock = new();

    private ChunkMesh? mesh;
    private ChunkMesh? toReplace;

    public ChunkRenderSlot(VoxelClient client) : base(client) {}

    public override void Render(double delta) {
        //DebugDraw(new vec4(1, 1, 1, 1));

        //Do nothing if this chunk render slot doesn't have a chunk yet, or if the chunk it does have is empty.
        if (targetChunk == null) {
            //DebugDraw(new vec4(0, 1, 1, 1));
            return;
        }

        if (targetChunk.isEmpty) {
            //DebugDraw(new vec4(0, 0, 0, 1));
            return;
        }

        if (lastVersion != targetChunk.GetVersion()) {
            Rebuild();
        }

        //Store this to prevent race conditions between == null and .render
        lock (MeshLock) {
            if (toReplace != null) {
                mesh?.Dispose();
                mesh = toReplace;
                toReplace = null;
            }

            if (mesh == null) {
                //DebugDraw(new vec4(1, 0, 1, 1));
                return;
            }

            //DebugDraw(new vec4(0, 1, 0, 1));
            mesh.Render();
        }
    }

    public void Move(ivec3 absolutePos, VoxelWorld world) {
        if (RealPosition == absolutePos)
            return;

        //DebugDraw(new vec4(0, 1, 0, 1));

        RealPosition = absolutePos;
        //Should never be null bc this only has 1 callsite that already null checks it
        targetChunk = world.GetOrCreateChunk(RealPosition);
        lastVersion = null;
    }

    public void SetMesh(ChunkMesh mesh) {
        lock (MeshLock) {
            toReplace = mesh;

            //Console.WriteLine($"Set mesh to {mesh} with {mesh.IndexCount} indecies");
        }
    }

    public override void Dispose() {
        lock (MeshLock) {
            mesh?.Dispose();
            mesh = null;
        }
    }

    public void DebugDraw(vec4 color) {
        if (targetChunk == null)
            return;

        DebugRenderer.SetColor(color);
        DebugRenderer.DrawCube(RealPosition * PositionExtensions.ChunkSize, (RealPosition + ivec3.Ones) * PositionExtensions.ChunkSize, -1);
    }


    public void Reload() {
        lock (MeshLock) {
            mesh?.Dispose();
            mesh = null;
        }

        lastVersion = null;
    }


    private void Rebuild() {
        if (!ChunkMeshBuilder.Rebuild(this, RealPosition)) {
            //DebugDraw(new vec4(1, 0, 0, 1));
            return;
        }

        //Console.Out.WriteLine("Rebuild");

        lastVersion = targetChunk!.GetVersion();
    }

    public class ChunkMesh : IDisposable {
        public readonly VoxelClient Client;
        public readonly RenderSystem RenderSystem;

        public readonly ivec3 Position;
        public readonly dvec3 WorldPosition;
        public readonly VertexBuffer<TerrainVertex.Packed> Buffer;
        public readonly uint IndexCount;
        
        public readonly Box MeshBox;

        private readonly GraphicsBuffer<ChunkMeshUniform> UniformBuffer;
        private readonly ResourceSet UniformResourceSet;

        public ChunkMesh(VoxelClient client, Span<TerrainVertex.Packed> packedVertices, uint indexCount, ivec3 position) {
            Client = client;
            RenderSystem = Client.renderSystem!;
            Buffer = new(RenderSystem);

            lock (Client.renderSystem!) {
                Buffer.UpdateDeferred(packedVertices);
            }
            IndexCount = indexCount;

            Position = position;
            WorldPosition = position.ChunkToWorldPosition();

            UniformBuffer = new(RenderSystem, GraphicsBufferUsage.UniformBuffer | GraphicsBufferUsage.Dynamic, 1);
            UniformResourceSet = RenderSystem.ResourceFactory.CreateResourceSet(new() {
                Layout = Client.gameRenderer!.WorldRenderer.ChunkRenderer.ChunkResourceLayout,
                BoundResources = [
                    UniformBuffer.BaseBuffer
                ]
            });

            MeshBox = new(position.ChunkToWorldPosition(), (position + 1).ChunkToWorldPosition());
        }

        public void Render() {
            //Just in case...
            if (Buffer == null)
                return;

            //Set up chunk transform relative to camera.
            UniformBuffer.UpdateImmediate(0, [new() {
                modelMatrix = mat4.Translate((vec3)(WorldPosition - CameraStateManager.currentCameraPosition)).Transposed
            }]);
            RenderSystem.MainCommandList.SetGraphicsResourceSet(1, UniformResourceSet);

            Buffer.Bind(0);
            RenderSystem.DrawIndexed(IndexCount);
        }

        public void Dispose() {
            Buffer.Dispose();
            UniformBuffer.Dispose();
            RenderSystem.GraphicsDevice.DisposeWhenIdle(UniformResourceSet);
        }


        private struct ChunkMeshUniform {
            public mat4 modelMatrix = mat4.Identity.Transposed;

            public ChunkMeshUniform() {
            }
        }
    }
}
