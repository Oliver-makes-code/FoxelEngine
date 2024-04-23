using System;
using System.Runtime.InteropServices;
using Veldrid;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering.Utils;

/// <summary>
/// TODO: Finish this?
/// </summary>
/// <typeparam name="T"></typeparam>
public class ArrayTypedDeviceBuffer<T> : IDisposable where T : unmanaged {

    private readonly int ElementSize = Marshal.SizeOf<T>();
    private readonly RenderSystem RenderSystem;
    private readonly DeviceBuffer Buffer;

    private T[] data;

    public ArrayTypedDeviceBuffer(BufferDescription description, RenderSystem system, int capacity) {
        RenderSystem = system;

        description.SizeInBytes = (uint)(capacity * ElementSize);
        Buffer = system.ResourceFactory.CreateBuffer(description);
        data = new T[capacity];
    }

    /*public void SetValue(T newValue, int index CommandList? commandList) {
        _data = newValue;

        if (commandList != null)
            commandList.UpdateBuffer(Buffer, 0, newValue);
        else
            RenderSystem.GraphicsDevice.UpdateBuffer(Buffer, 0, newValue);
    }

    public void CopyFrom(Span<T> source, int index) {

    }*/

    public void Dispose() {
        Buffer.Dispose();
    }
}
