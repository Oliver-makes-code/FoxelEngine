using System.Collections.Concurrent;
using Veldrid;
using Foxel.Core.Assets;

namespace Foxel.Core.Rendering;

public class RenderSystem {
    public const uint QuadCount = 196608;

    public readonly Game Game;

    public readonly CommandList MainCommandList;

    public readonly TextureManager TextureManager;

    public readonly ShaderManager ShaderManager;

    /// <summary>
    /// This index buffer is a 'common' index buffer used by potentially many objects.
    ///
    /// Any object that uses a quad-driven triangle list can simply use this index buffer instead of creating their own.
    /// It supports up to <see cref="QuadCount"/> quads.
    /// </summary>
    public readonly DeviceBuffer CommonIndexBuffer;

    public GraphicsDevice GraphicsDevice => Game.graphicsDevice!;
    public ResourceFactory ResourceFactory => GraphicsDevice.ResourceFactory;


    public ConcurrentStack<IDisposable> disposeQueue = new();

    public RenderSystem(Game game, PackManager packManager) {
        Game = game;

        TextureManager = new(this, packManager);
        ShaderManager = new(this, packManager);

        game.nativeWindow!.Resized += NativeWindowOnResized;

        MainCommandList = ResourceFactory.CreateCommandList();
        MainCommandList.Begin();

        uint[] commonBufferData = new uint[QuadCount * 6];

        CommonIndexBuffer = ResourceFactory.CreateBuffer(new() {
            Usage = BufferUsage.IndexBuffer,
            SizeInBytes = sizeof(uint) * QuadCount * 6
        });

        uint indexIndex = 0;
        for (uint i = 0; i < QuadCount; i++) {
            uint vertexIndex = i * 4;

            commonBufferData[indexIndex++] = vertexIndex;
            commonBufferData[indexIndex++] = vertexIndex + 1;
            commonBufferData[indexIndex++] = vertexIndex + 2;

            commonBufferData[indexIndex++] = vertexIndex + 2;
            commonBufferData[indexIndex++] = vertexIndex + 3;
            commonBufferData[indexIndex++] = vertexIndex;
        }

        GraphicsDevice.UpdateBuffer(CommonIndexBuffer, 0, commonBufferData);
    }

    public void RestartCommandBuffer() {
        MainCommandList.End();
        GraphicsDevice.SubmitCommands(MainCommandList);
        while (disposeQueue.TryPop(out var d))
            d.Dispose();
        MainCommandList.Begin();
    }
    
    public void Dispose(IDisposable obj) => disposeQueue.Push(obj);

    private void NativeWindowOnResized() {
        GraphicsDevice.ResizeMainWindow((uint)Game.nativeWindow!.Width, (uint)Game.nativeWindow.Height);
    }

    internal void EndFrame() {
        RestartCommandBuffer();
        GraphicsDevice.SwapBuffers();
    }
}
