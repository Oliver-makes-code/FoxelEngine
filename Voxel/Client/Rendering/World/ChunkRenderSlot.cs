using System;
using System.Runtime.InteropServices;
using GlmSharp;
using RenderSurface.Rendering;
using Veldrid;
using Voxel.Client.Rendering.Utils;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Common.Util;
using Voxel.Common.World;

namespace Voxel.Client.Rendering.World;

/// <summary>
/// Holds the mesh data for a single chunk, its render state, etc, etc.
/// </summary>
public class ChunkRenderSlot : Renderer {

    public readonly ivec3 RelativePosition;
    private readonly object MeshLock = new object();


    private ivec3 realPosition;
    public uint? lastVersion;
    private ChunkMesh? mesh;

    public Chunk? TargetChunk { get; private set; }

    public ChunkRenderSlot(VoxelNewClient client, ivec3 relativePosition) : base(client) {
        RelativePosition = relativePosition;
    }

    public override void Render(double delta) {

        //Do nothing if this chunk render slot doesn't have a chunk yet, or if the chunk it does have is empty.
        if (TargetChunk == null || TargetChunk.IsEmpty)
            return;

        if (lastVersion != TargetChunk.GetVersion())
            Rebuild();

        //Store this to prevent race conditions between == null and .render
        lock (MeshLock) {
            if (mesh == null)
                return;
            mesh.Render();
        }
    }

    public void Move(ivec3 newCenterPos) {
        realPosition = newCenterPos + RelativePosition;

        //Should never be null bc this only has 1 callsite that already null checks it
        TargetChunk = Client.World!.GetOrCreateChunk(realPosition);
        lastVersion = null;
    }


    private void Rebuild() {
        if (!ChunkMeshBuilder.Rebuild(this, realPosition))
            return;

        lastVersion = TargetChunk!.GetVersion();
    }

    public void SetMesh(ChunkMesh mesh) {
        lock (MeshLock) {
            this.mesh?.Dispose(); //Dispose of old, if it exists.
            this.mesh = mesh; //Slot in new.
        }
    }

    public override void Dispose() {
        mesh?.Dispose();
    }

    public class ChunkMesh : IDisposable {
        public readonly VoxelNewClient Client;
        public readonly RenderSystem RenderSystem;

        public readonly ivec3 Position;
        public readonly dvec3 WorldPosition;
        public readonly DeviceBuffer? Buffer;
        public readonly uint IndexCount;

        private readonly TypedDeviceBuffer<ChunkMeshUniform> UniformBuffer;
        private readonly ResourceSet UniformResourceSet;

        public ChunkMesh(VoxelNewClient client, Span<BasicVertex.Packed> packedVertices, uint indexCount, ivec3 position) {
            Client = client;
            RenderSystem = Client.RenderSystem;

            Buffer = RenderSystem.ResourceFactory.CreateBuffer(new BufferDescription {
                SizeInBytes = (uint)Marshal.SizeOf<BasicVertex.Packed>() * (uint)packedVertices.Length,
                Usage = BufferUsage.VertexBuffer
            });
            RenderSystem.GraphicsDevice.UpdateBuffer(Buffer, 0, packedVertices);
            IndexCount = indexCount;

            Position = position;
            WorldPosition = position.ChunkToWorldPosition();

            UniformBuffer = new TypedDeviceBuffer<ChunkMeshUniform>(new BufferDescription { Usage = BufferUsage.Dynamic | BufferUsage.UniformBuffer }, RenderSystem);
            UniformResourceSet = RenderSystem.ResourceFactory.CreateResourceSet(new ResourceSetDescription {
                Layout = Client.GameRenderer.WorldRenderer.ChunkRenderer.ChunkResourceLayout,
                BoundResources = new BindableResource[] {
                    UniformBuffer.BackingBuffer
                }
            });
        }

        public void Render() {
            //Just in case...
            if (Buffer == null)
                return;

            //Set up chunk transform relative to camera.
            UniformBuffer.SetValue(new ChunkMeshUniform {
                ModelMatrix = mat4.Translate((vec3)(WorldPosition - CameraStateManager.currentCameraPosition)).Transposed
            });
            RenderSystem.MainCommandList.SetGraphicsResourceSet(0, Client.GameRenderer.CameraStateManager.CameraResourceSet);
            RenderSystem.MainCommandList.SetGraphicsResourceSet(1, UniformResourceSet);

            RenderSystem.MainCommandList.SetVertexBuffer(0, Buffer);
            RenderSystem.MainCommandList.DrawIndexed(IndexCount);
        }

        public void Dispose() {
            RenderSystem.GraphicsDevice.DisposeWhenIdle(Buffer);
        }


        private struct ChunkMeshUniform {
            public mat4 ModelMatrix = mat4.Identity.Transposed;


            public ChunkMeshUniform() {
            }
        }
    }
}
