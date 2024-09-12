using System.Runtime.InteropServices;
using Veldrid;

namespace Foxel.Core.Rendering.Buffer;

public sealed class GraphicsBuffer<T> : IDisposable where T : unmanaged {
    public readonly uint Size;
    public readonly RenderSystem RenderSystem;
    public readonly DeviceBuffer BaseBuffer;

    public GraphicsBuffer(RenderSystem renderSystem, GraphicsBufferUsage usage, uint size, uint stride = 0) {
        RenderSystem = renderSystem;
        Size = size;
        uint calculatedSize = Size * (uint)Marshal.SizeOf<T>();
        calculatedSize += 16 - (calculatedSize % 16);
        BaseBuffer = RenderSystem.ResourceFactory.CreateBuffer(new(calculatedSize, usage.BaseBufferUsage(), stride));
    }

    public void UpdateDeferred(uint start, Span<T> data)
        => RenderSystem.GraphicsDevice.UpdateBuffer(BaseBuffer, start, data);

    public void UpdateImmediate(uint start, Span<T> data)
        => RenderSystem.MainCommandList.UpdateBuffer(BaseBuffer, start, data);

    public void Dispose() {
        RenderSystem.GraphicsDevice.DisposeWhenIdle(BaseBuffer);
    }
}

public static class GraphicsBufferExtensions {
    public static void UpdateDeferred<T>(this GraphicsBuffer<T> buffer, uint start, VertexConsumer<T> consumer) where T : unmanaged, Vertex<T>
        => buffer.UpdateDeferred(start, consumer.AsSpan());
    public static void UpdateImmediate<T>(this GraphicsBuffer<T> buffer, uint start, VertexConsumer<T> consumer) where T : unmanaged, Vertex<T>
        => buffer.UpdateImmediate(start, consumer.AsSpan());
}

[Flags]
public enum GraphicsBufferUsage : byte {
    VertexBuffer = 0x01,
    IndexBuffer = 0x02,
    UniformBuffer = 0x04,
    StructuredBufferReadOnly = 0x08,
    StructuredBufferReadWrite = 0x10,
    IndirectBuffer = 0x20,
    Dynamic = 0x40,
    Staging = 0x80
}

internal static class GraphicsBufferUsageExtensions {
    public static BufferUsage BaseBufferUsage(this GraphicsBufferUsage graphicsBufferUsage)
        => (BufferUsage)graphicsBufferUsage;
}
