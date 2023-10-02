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

    public RenderSystem(Game game, AssetReader assetReader) {
        Game = game;

        GraphicsDevice = game.GraphicsDevice;
        ResourceFactory = GraphicsDevice.ResourceFactory;

        TextureManager = new(this, assetReader);
        ShaderManager = new(this, assetReader);

        game.NativeWindow.Resized += NativeWindowOnResized;

        MainCommandList = ResourceFactory.CreateCommandList();
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
