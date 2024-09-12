using System.Runtime.InteropServices;
using Veldrid;

namespace Foxel.Core.Rendering.Buffer;

public sealed class GraphicsBuffer : IDisposable {
    public readonly GraphicsBufferUsage Usage;
    public readonly RenderSystem RenderSystem;
    private readonly uint Stride;
    public DeviceBuffer? baseBuffer;
    public uint size { get; private set; } = 0;
    private uint capacity = 0;

    public GraphicsBuffer(RenderSystem renderSystem, GraphicsBufferUsage usage, uint stride = 0) {
        RenderSystem = renderSystem;
        Usage = usage;
        Stride = stride;
    }

    public void WithCapacity<T>(uint count) where T : unmanaged {
        uint calculatedSize = count * (uint)Marshal.SizeOf<T>();
        calculatedSize += 16 - (calculatedSize % 16);
        if (capacity >= calculatedSize && baseBuffer != null)
            return;
        baseBuffer?.Dispose();
        baseBuffer = RenderSystem.ResourceFactory.CreateBuffer(new(calculatedSize, Usage.BaseBufferUsage(), Stride));
        capacity = calculatedSize;
    }

    public void UpdateDeferred<T>(uint start, Span<T> data) where T : unmanaged {
        uint calculatedSize = (uint)data.Length * (uint)Marshal.SizeOf<T>();
        WithCapacity<T>((uint)data.Length);
        RenderSystem.GraphicsDevice.UpdateBuffer(baseBuffer, start, data);
        size = calculatedSize;
    }

    public void UpdateImmediate<T>(uint start, Span<T> data) where T : unmanaged {
        uint calculatedSize = (uint)data.Length * (uint)Marshal.SizeOf<T>();
        WithCapacity<T>((uint)data.Length);
        RenderSystem.MainCommandList.UpdateBuffer(baseBuffer, start, data);
        size = calculatedSize;
    }

    public void UpdateDeferred<T>(uint start, VertexConsumer<T> consumer) where T : unmanaged, Vertex<T>
        => UpdateDeferred(start, consumer.AsSpan());

    public void UpdateImmediate<T>(uint start, VertexConsumer<T> consumer) where T : unmanaged, Vertex<T>
        => UpdateImmediate(start, consumer.AsSpan());

    public void Dispose() {
        if (baseBuffer != null)
            RenderSystem.GraphicsDevice.DisposeWhenIdle(baseBuffer);
    }
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
