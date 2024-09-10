using System;
using System.Runtime.InteropServices;
using Veldrid;
using Foxel.Core.Rendering;

namespace Foxel.Client.Rendering.Utils;

public class TypedDeviceBuffer<T> : IDisposable where T : unmanaged {
    public readonly DeviceBuffer BackingBuffer;
    private readonly RenderSystem RenderSystem;

    public T value {
        get => _value;
        set => SetValue(value, RenderSystem.MainCommandList);
    }
    private T _value;

    public TypedDeviceBuffer(BufferDescription description, RenderSystem system, uint count = 1) {
        RenderSystem = system;
        description.SizeInBytes = (uint)(Math.Ceiling(Marshal.SizeOf<T>() * count / 16.0f) * 16.0f);
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
