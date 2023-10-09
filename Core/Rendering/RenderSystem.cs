using System.Collections.Concurrent;
using RenderSurface.Assets;
using Veldrid;

namespace RenderSurface.Rendering;

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

    public GraphicsDevice GraphicsDevice => Game.GraphicsDevice;
    public ResourceFactory ResourceFactory => GraphicsDevice.ResourceFactory;


    public ConcurrentStack<IDisposable> disposeQueue = new();

    public RenderSystem(Game game, AssetReader assetReader) {
        Game = game;

        TextureManager = new(this, assetReader);
        ShaderManager = new(this, assetReader);

        game.NativeWindow.Resized += NativeWindowOnResized;

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

    internal void StartFrame(double delta) {
        MainCommandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
        MainCommandList.ClearColorTarget(0, RgbaFloat.Grey);
        MainCommandList.ClearDepthStencil(1, 0);
    }

    internal void EndFrame() {
        RestartCommandBuffer();
        GraphicsDevice.SwapBuffers();
    }

    public void RestartCommandBuffer() {
        MainCommandList.End();
        GraphicsDevice.SubmitCommands(MainCommandList);
        while (disposeQueue.TryPop(out var d))
            d.Dispose();
        MainCommandList.Begin();
    }

    private void NativeWindowOnResized() {
        GraphicsDevice.ResizeMainWindow((uint)Game.NativeWindow.Width, (uint)Game.NativeWindow.Height);
    }
    public void Dispose(IDisposable obj) => disposeQueue.Push(obj);
}
