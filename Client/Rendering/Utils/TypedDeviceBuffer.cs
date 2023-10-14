using System;
using System.Runtime.InteropServices;
using RenderSurface.Rendering;
using Veldrid;

namespace Voxel.Client.Rendering.Utils;

public class TypedDeviceBuffer<T> : IDisposable where T : unmanaged {

    private readonly RenderSystem RenderSystem;
    public readonly DeviceBuffer BackingBuffer;

    private T _value;

    public T value {
        get => _value;
        set => SetValue(value, RenderSystem.MainCommandList);
    }

    public TypedDeviceBuffer(BufferDescription description, RenderSystem system) {
        RenderSystem = system;
        description.SizeInBytes = (uint)Marshal.SizeOf<T>();
        BackingBuffer = system.ResourceFactory.CreateBuffer(description);
    }

    public void SetValue(T newValue, CommandList? commandList = null) {
        _value = newValue;

        if (commandList != null)
            commandList.UpdateBuffer(BackingBuffer, 0, newValue);
        else
            RenderSystem.GraphicsDevice.UpdateBuffer(BackingBuffer, 0, newValue);
    }

    public void SetValue(T newValue, uint byteCount, CommandList? commandList = null) {
        _value = newValue;

        if (commandList != null)
            commandList.UpdateBuffer(BackingBuffer, 0, ref newValue, byteCount);
        else
            RenderSystem.GraphicsDevice.UpdateBuffer(BackingBuffer, 0, ref newValue, byteCount);
    }

    public void Dispose() {
        BackingBuffer.Dispose();
    }
}