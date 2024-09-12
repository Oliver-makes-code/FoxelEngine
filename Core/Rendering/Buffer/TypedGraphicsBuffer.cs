using System.Runtime.InteropServices;
using Veldrid;

namespace Foxel.Core.Rendering.Buffer;

public sealed class TypedGraphicsBuffer<T> : IDisposable where T : unmanaged {
    public readonly GraphicsBuffer Buffer;
    public DeviceBuffer? baseBuffer => Buffer.baseBuffer;
    public uint size => (uint)(Buffer.size / Marshal.SizeOf<T>());

    public TypedGraphicsBuffer(RenderSystem renderSystem, GraphicsBufferUsage usage, uint stride = 0) {
        Buffer = new(renderSystem, usage, stride);
    }

    public void WithCapacity(uint capacity)
        => Buffer.WithCapacity<T>(capacity);

    public void UpdateDeferred(uint start, Span<T> data)
        => Buffer.UpdateDeferred(start, data);

    public void UpdateImmediate(uint start, Span<T> data)
        => Buffer.UpdateImmediate(start, data);

    public void Dispose() {
        Buffer.Dispose();
    }
}

public static class TypedDeviceBufferExtensions {
    public static void UpdateDeferred<T>(this TypedGraphicsBuffer<T> buffer, uint start, VertexConsumer<T> consumer) where T : unmanaged, Vertex<T>
        => buffer.Buffer.UpdateDeferred(start, consumer);
    public static void UpdateImmediate<T>(this TypedGraphicsBuffer<T> buffer, uint start, VertexConsumer<T> consumer) where T : unmanaged, Vertex<T>
        => buffer.Buffer.UpdateImmediate(start, consumer);
}
