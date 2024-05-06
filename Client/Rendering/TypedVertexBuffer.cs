using System.Runtime.InteropServices;
using Veldrid;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering;

/// <summary>
/// A typed, dynamically sized vertex buffer.
/// </summary>
/// <typeparam name="TVertex">The vertex type for the buffer</typeparam>
public class TypedVertexBuffer<TVertex> where TVertex : unmanaged, Vertex<TVertex> {
    public DeviceBuffer buffer { get; private set; }
    public uint size { get; private set; } = 0;
    private uint capacity = 1;

    public TypedVertexBuffer(ResourceFactory resourceFactory) {
        buffer = RebuildBuffer(resourceFactory);
    }

    public void Update(VertexConsumer<TVertex> vertices, CommandList commandList, ResourceFactory resourceFactory) {
        bool shouldRebuild = capacity < vertices.Count;
        while (capacity < vertices.Count)
            IncreaseCapacity();
        if (shouldRebuild)
            buffer = RebuildBuffer(resourceFactory);
        commandList.UpdateBuffer(buffer, 0, vertices.AsSpan());
        size = (uint)vertices.Count;
    }

    private void IncreaseCapacity() {
        capacity *= 2;
    }

    private DeviceBuffer RebuildBuffer(ResourceFactory resourceFactory) {
        buffer?.Dispose();
        return resourceFactory.CreateBuffer(new() {
            SizeInBytes = (uint)(Marshal.SizeOf<TVertex>() * capacity),
            Usage = BufferUsage.VertexBuffer | BufferUsage.Dynamic
        });
    }
}