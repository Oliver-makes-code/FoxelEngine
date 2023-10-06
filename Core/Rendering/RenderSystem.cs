using RenderSurface.Assets;
using Veldrid;

namespace RenderSurface.Rendering;

public class RenderSystem {
    public Game Game { get; private set; }

    public GraphicsDevice GraphicsDevice { get; private set; }
    public ResourceFactory ResourceFactory { get; private set; }

    public CommandList MainCommandList { get; private set; }

    public TextureManager TextureManager { get; private set; }
    public ShaderManager ShaderManager { get; private set; }

    /// <summary>
    /// This index buffer is a 'common' index buffer used by potentially many objects.
    ///
    /// Any object that uses a quad-driven triangle list can simply use this index buffer instead of creating their own.
    /// It supports up to 196608 quads.
    /// </summary>
    public DeviceBuffer CommonIndexBuffer { get; private set; }

    public RenderSystem(Game game, AssetReader assetReader) {
        Game = game;

        GraphicsDevice = game.GraphicsDevice;
        ResourceFactory = GraphicsDevice.ResourceFactory;

        TextureManager = new(this, assetReader);
        ShaderManager = new(this, assetReader);

        game.NativeWindow.Resized += NativeWindowOnResized;

        MainCommandList = ResourceFactory.CreateCommandList();

        var quadCount = 196608u;
        uint[] commonBufferData = new uint[quadCount * 6];

        CommonIndexBuffer = ResourceFactory.CreateBuffer(new BufferDescription {
            Usage = BufferUsage.IndexBuffer,
            SizeInBytes = sizeof(uint) * quadCount * 6
        });

        var indexIndex = 0u;
        for (uint i = 0; i < quadCount; i++) {
            var vertexIndex = i * 4;

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
        MainCommandList.Begin();
        MainCommandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
        MainCommandList.ClearColorTarget(0, RgbaFloat.Grey);
    }

    internal void EndFrame() {
        MainCommandList.End();
        GraphicsDevice.SubmitCommands(MainCommandList);
        GraphicsDevice.SwapBuffers();
    }


    private void NativeWindowOnResized() {
        GraphicsDevice.ResizeMainWindow((uint)Game.NativeWindow.Width, (uint)Game.NativeWindow.Height);
    }
}
