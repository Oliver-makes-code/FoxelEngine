using System.Runtime.InteropServices;
using Veldrid;

namespace Foxel.Core.Rendering.Buffer;

public sealed class VertexBuffer<TVertex> : IDisposable where TVertex : unmanaged, Vertex<TVertex> {
    public readonly RenderSystem RenderSystem;
    public DeviceBuffer? baseBuffer { get; private set ;}
    public uint size { get; private set; }
    
    public VertexBuffer(RenderSystem renderSystem) {
        RenderSystem = renderSystem;
    }

    public void UpdateDeferred(Span<TVertex> vertices) {
        baseBuffer = RebuildBuffer((uint)vertices.Length);
        RenderSystem.GraphicsDevice.UpdateBuffer(baseBuffer, 0, vertices);
        size = (uint)vertices.Length;
    }

    public void UpdateImmediate(Span<TVertex> vertices) {
        size = (uint)vertices.Length;
        baseBuffer = RebuildBuffer(size);
        RenderSystem.MainCommandList.UpdateBuffer(baseBuffer, 0, vertices);
    }

    public void UpdateImmediate(VertexConsumer<TVertex> consumer)
        => UpdateImmediate(consumer.AsSpan());

    public void UpdateDeferred(VertexConsumer<TVertex> consumer)
        => UpdateDeferred(consumer.AsSpan());

    public void Bind(uint index) {
        RenderSystem.MainCommandList.SetVertexBuffer(index, baseBuffer);
    }

    public void Dispose() {
        if (baseBuffer != null)
            RenderSystem.GraphicsDevice.DisposeWhenIdle(baseBuffer);
    }

    private DeviceBuffer RebuildBuffer(uint size) {
        if (baseBuffer != null)
            RenderSystem.GraphicsDevice.DisposeWhenIdle(baseBuffer);
        uint calculatedSize = (uint)(Marshal.SizeOf<TVertex>() * size);
        calculatedSize += 16 - (calculatedSize % 16);
        return RenderSystem.ResourceFactory.CreateBuffer(new() {
            SizeInBytes = calculatedSize,
            Usage = BufferUsage.VertexBuffer | BufferUsage.Dynamic
        });
    }
}