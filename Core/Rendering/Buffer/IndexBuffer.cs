using System.Runtime.InteropServices;
using Veldrid;

namespace Foxel.Core.Rendering.Buffer;

public sealed class IndexBuffer {
    public readonly RenderSystem RenderSystem;
    public DeviceBuffer? baseBuffer { get; private set ;}
    public uint size { get; private set; }
    
    public IndexBuffer(RenderSystem renderSystem) {
        RenderSystem = renderSystem;
    }

    public void Update(Span<uint> data) {
        size = (uint)data.Length;
        baseBuffer = RebuildBuffer(size);
        RenderSystem.GraphicsDevice.UpdateBuffer(baseBuffer, 0, data);
    }

    public void Bind() {
        RenderSystem.MainCommandList.SetIndexBuffer(baseBuffer, IndexFormat.UInt32, 0);
    }

    private DeviceBuffer RebuildBuffer(uint size) {
        if (baseBuffer != null)
            RenderSystem.GraphicsDevice.DisposeWhenIdle(baseBuffer);
        uint calculatedSize = sizeof(uint) * size;
        calculatedSize += 16 - (calculatedSize % 16);
        return RenderSystem.ResourceFactory.CreateBuffer(new() {
            SizeInBytes = calculatedSize,
            Usage = BufferUsage.IndexBuffer | BufferUsage.Dynamic
        });
    }
}
