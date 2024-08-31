using System.Runtime.InteropServices;
using Veldrid;
using Foxel.Core.Rendering;

namespace Foxel.Client.Rendering;

/// <summary>
/// A typed, dynamically sized vertex buffer.
/// </summary>
/// <typeparam name="TVertex">The vertex type for the buffer</typeparam>
public class TypedVertexBuffer<TVertex> where TVertex : unmanaged, Vertex<TVertex> {
    public DeviceBuffer buffer { get; private set; }
    public uint size { get; private set; } = 0;

    public TypedVertexBuffer(ResourceFactory resourceFactory) {
        buffer = RebuildBuffer(resourceFactory);
    }

    public void Update(VertexConsumer<TVertex> vertices, CommandList commandList, ResourceFactory resourceFactory) {
        size = (uint)vertices.Count;
        buffer = RebuildBuffer(resourceFactory);
        commandList.UpdateBuffer(buffer, 0, vertices.AsSpan());
    }

    private DeviceBuffer RebuildBuffer(ResourceFactory resourceFactory) {
        buffer?.Dispose();
        return resourceFactory.CreateBuffer(new() {
            SizeInBytes = (uint)(Marshal.SizeOf<TVertex>() * size),
            Usage = BufferUsage.VertexBuffer | BufferUsage.Dynamic
        });
    }
}
