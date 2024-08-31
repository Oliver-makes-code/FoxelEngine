using System;
using System.Runtime.InteropServices;
using GlmSharp;
using Veldrid;
using Foxel.Client.Rendering.Debug;
using Foxel.Client.Rendering.Utils;
using Foxel.Client.Rendering.VertexTypes;
using Foxel.Common.Collision;
using Foxel.Common.Util;
using Foxel.Common.World;
using Foxel.Core;
using Foxel.Core.Rendering;

namespace Foxel.Client.Rendering.World;

/// <summary>
/// Holds the mesh data for a single chunk, its render state, etc, etc.
/// </summary>
public class ChunkRenderSlot : Renderer {

    public uint? lastVersion;
    public Chunk? targetChunk { get; private set; }

    public ivec3 RealPosition { get; private set; } = ivec3.MinValue;

    private readonly object MeshLock = new();

    private ChunkMesh? mesh;

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
            this.mesh?.Dispose(); //Dispose of old, if it exists.
            this.mesh = mesh; //Slot in new.

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
        public readonly DeviceBuffer? Buffer;
        public readonly uint IndexCount;
        
        public readonly Box MeshBox;

        private readonly TypedDeviceBuffer<ChunkMeshUniform> UniformBuffer;
        private readonly ResourceSet UniformResourceSet;

        public ChunkMesh(VoxelClient client, Span<TerrainVertex.Packed> packedVertices, uint indexCount, ivec3 position) {
            Client = client;
            RenderSystem = Client.renderSystem!;

            lock (Client.renderSystem!) {
                Buffer = RenderSystem.ResourceFactory.CreateBuffer(new() {
                    SizeInBytes = (uint)Marshal.SizeOf<TerrainVertex.Packed>() * (uint)packedVertices.Length, Usage = BufferUsage.VertexBuffer
                });
                RenderSystem.GraphicsDevice.UpdateBuffer(Buffer, 0, packedVertices);
            }
            IndexCount = indexCount;

            Position = position;
            WorldPosition = position.ChunkToWorldPosition();

            UniformBuffer = new(new() {
                Usage = BufferUsage.Dynamic | BufferUsage.UniformBuffer
            }, RenderSystem);
            UniformResourceSet = RenderSystem.ResourceFactory.CreateResourceSet(new() {
                Layout = Client.gameRenderer!.WorldRenderer.ChunkRenderer.ChunkResourceLayout,
                BoundResources = [
                    UniformBuffer.BackingBuffer
                ]
            });

            MeshBox = new(position.ChunkToWorldPosition(), (position + 1).ChunkToWorldPosition());
        }

        public void Render() {
            //Just in case...
            if (Buffer == null)
                return;

            //Set up chunk transform relative to camera.
            UniformBuffer.SetValue(new() {
                modelMatrix = mat4.Translate((vec3)(WorldPosition - CameraStateManager.currentCameraPosition)).Transposed
            });
            RenderSystem.MainCommandList.SetGraphicsResourceSet(1, UniformResourceSet);

            RenderSystem.MainCommandList.SetVertexBuffer(0, Buffer);
            RenderSystem.MainCommandList.DrawIndexed(IndexCount);
        }

        public void Dispose() {
            RenderSystem.GraphicsDevice.DisposeWhenIdle(Buffer);
            RenderSystem.GraphicsDevice.DisposeWhenIdle(UniformBuffer);
            RenderSystem.GraphicsDevice.DisposeWhenIdle(UniformResourceSet);
        }


        private struct ChunkMeshUniform {
            public mat4 modelMatrix = mat4.Identity.Transposed;

            public ChunkMeshUniform() {
            }
        }
    }
}
