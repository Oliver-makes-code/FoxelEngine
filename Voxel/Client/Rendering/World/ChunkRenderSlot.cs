using System;
using System.Runtime.InteropServices;
using GlmSharp;
using RenderSurface.Rendering;
using Veldrid;
using Voxel.Client.Rendering.VertexTypes;
using Voxel.Common.World;

namespace Voxel.Client.Rendering.World;

/// <summary>
/// Holds the mesh data for a single chunk, its render state, etc, etc.
/// </summary>
public class ChunkRenderSlot : Renderer {

    public readonly ivec3 RelativePosition;
    public Chunk? TargetChunk { get; private set; }

    public uint? _lastVersion;

    private object _meshLock = new object();
    private ChunkMesh? Mesh;

    public ChunkRenderSlot(VoxelNewClient client, ivec3 relativePosition) : base(client) {
        RelativePosition = relativePosition;
    }

    public override void Render(double delta) {

        //Do nothing if this chunk render slot doesn't have a chunk yet, or if the chunk it does have is empty.
        if (TargetChunk == null || TargetChunk.IsEmpty)
            return;

        if (_lastVersion != TargetChunk.GetVersion())
            Rebuild();

        //Store this to prevent race conditions between == null and .render
        lock (_meshLock) {
            if (Mesh == null)
                return;
            Mesh.Render();
        }
    }

    public void Move(ivec3 newCenterPos) {
        //Should never be null bc this only has 1 callsite that already null checks it
        TargetChunk = Client.World!.GetOrCreateChunk(newCenterPos + RelativePosition);
        _lastVersion = null;
    }


    private void Rebuild() {
        if (!ChunkMeshBuilder.Rebuild(this))
            return;

        _lastVersion = TargetChunk!.GetVersion();
    }

    public void SetMesh(ChunkMesh mesh) {
        lock (_meshLock) {
            Mesh?.Dispose(); //Dispose of old, if it exists.
            Mesh = mesh; //Slot in new.
        }
    }

    public override void Dispose() {
        Mesh?.Dispose();
    }

    public class ChunkMesh : IDisposable {
        public readonly RenderSystem RenderSystem;

        public ivec3 Position;
        public DeviceBuffer? Buffer;
        public uint IndexCount;

        public ChunkMesh(RenderSystem renderSystem) {
            RenderSystem = renderSystem;
        }

        public void SetBuffer(Span<BasicVertex.Packed> packedVertices, uint indexCount) {
            if (Buffer != null)
                throw new Exception("Cannot update buffer on already built mesh");

            Buffer = RenderSystem.ResourceFactory.CreateBuffer(new BufferDescription {
                SizeInBytes = (uint)Marshal.SizeOf<BasicVertex.Packed>() * (uint)packedVertices.Length,
                Usage = BufferUsage.VertexBuffer
            });

            RenderSystem.GraphicsDevice.UpdateBuffer(Buffer, 0, packedVertices);
        }

        public void Render() {

            //Just in case...
            if (Buffer == null)
                return;

            //TODO - Generate translation matrix and put that in a resource set...

            RenderSystem.MainCommandList.SetVertexBuffer(0, Buffer);
            RenderSystem.MainCommandList.DrawIndexed(IndexCount);
        }

        public void Dispose() {
            RenderSystem.GraphicsDevice.DisposeWhenIdle(Buffer);
        }
    }
}
