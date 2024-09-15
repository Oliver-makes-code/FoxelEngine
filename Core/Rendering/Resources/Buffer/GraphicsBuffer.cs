using System.Runtime.InteropServices;
using Veldrid;

namespace Foxel.Core.Rendering.Resources.Buffer;

public sealed class GraphicsBuffer<T> : IDisposable where T : unmanaged {
    public readonly uint Size;
    public readonly RenderSystem RenderSystem;
    public readonly DeviceBuffer BaseBuffer;
    public readonly GraphicsBufferType Type;

    public GraphicsBuffer(RenderSystem renderSystem, GraphicsBufferType type, uint size, uint stride = 0) {
        RenderSystem = renderSystem;
        Size = size;
        Type = type;
        uint calculatedSize = Size * (uint)Marshal.SizeOf<T>();
        calculatedSize += 16 - (calculatedSize % 16);
        BaseBuffer = RenderSystem.ResourceFactory.CreateBuffer(new(calculatedSize, Type.GetBufferUsage(), stride));
    }

    public void UpdateDeferred(uint start, Span<T> data) {
        if (!Game.isOpen)
            return;

        RenderSystem.GraphicsDevice.UpdateBuffer(BaseBuffer, start, data);
    }

    public void UpdateImmediate(uint start, Span<T> data) {
        if (!Game.isOpen)
            return;

        RenderSystem.MainCommandList.UpdateBuffer(BaseBuffer, start, data);
    }

    public void Dispose() {
        if (!Game.isOpen)
            return;

        RenderSystem.GraphicsDevice.DisposeWhenIdle(BaseBuffer);
    }
}

public static class GraphicsBufferExtensions {
    public static void UpdateDeferred<T>(this GraphicsBuffer<T> buffer, uint start, VertexConsumer<T> consumer) where T : unmanaged, Vertex<T>
        => buffer.UpdateDeferred(start, consumer.AsSpan());
    public static void UpdateImmediate<T>(this GraphicsBuffer<T> buffer, uint start, VertexConsumer<T> consumer) where T : unmanaged, Vertex<T>
        => buffer.UpdateImmediate(start, consumer.AsSpan());
}

public enum GraphicsBufferType : byte {
    UniformBuffer,
    StructuredBufferReadOnly,
    StructuredBufferReadWrite
}

internal static class GraphicsBufferUsageExtensions {
    public static BufferUsage GetBufferUsage(this GraphicsBufferType type)
        => type switch {
            GraphicsBufferType.UniformBuffer => BufferUsage.UniformBuffer,
            GraphicsBufferType.StructuredBufferReadOnly => BufferUsage.StructuredBufferReadOnly,
            GraphicsBufferType.StructuredBufferReadWrite => BufferUsage.StructuredBufferReadWrite,
            _ => 0,
        } | BufferUsage.Dynamic;
    
    public static ResourceKind GetResourceKind(this GraphicsBufferType type)
        => type switch {
            GraphicsBufferType.UniformBuffer => ResourceKind.UniformBuffer,
            GraphicsBufferType.StructuredBufferReadOnly => ResourceKind.StructuredBufferReadOnly,
            GraphicsBufferType.StructuredBufferReadWrite => ResourceKind.StructuredBufferReadWrite,
            _ => 0,
        };
}
