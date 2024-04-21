using System;
using System.Runtime.InteropServices;
using Veldrid;
using Voxel.Core.Rendering;

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
        description.SizeInBytes = (uint)(Math.Ceiling(Marshal.SizeOf<T>() / 16.0f) * 16.0f);
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

    public static implicit operator DeviceBuffer (TypedDeviceBuffer<T> buffer)
        => buffer.BackingBuffer;
}
