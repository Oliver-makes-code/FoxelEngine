using GlmSharp;
using Veldrid;
using Voxel.Client.Rendering.Debug;
using Voxel.Client.Rendering.Gui;
using Voxel.Client.Rendering.World;
using Voxel.Core.Assets;
using Voxel.Core.Rendering;

namespace Voxel.Client.Rendering;

public class GameRenderer : Renderer {
    /// <summary>
    /// Main camera, used to render main game window.
    /// Cannot be destroyed, it's essential for basic game rendering.
    /// </summary>
    public readonly Camera MainCamera;
    public readonly CameraStateManager CameraStateManager;


    public readonly WorldRenderer WorldRenderer;
    public readonly GuiRenderer GuiRenderer;

    public readonly BlitRenderer BlitRenderer;
    public readonly DebugRenderer DebugRenderer;
    public readonly ImGuiRenderDispatcher ImGuiRenderDispatcher;

    public readonly PackManager.ReloadTask ReloadTask;

    public readonly PackManager.ReloadTask FrameBufferTask;

    public MainFramebuffer? frameBuffer { get; private set; }

    private uint msaaLevel = (uint)ClientConfig.General.msaaLevel;
    private bool needMainBufferRefresh = true;
    

    public GameRenderer(VoxelClient client) : base(client) {
        //Jank but OK
        client.gameRenderer = this;

        MainCamera = new();
        CameraStateManager = new(RenderSystem);

        WorldRenderer = new(Client);
        DependsOn(WorldRenderer);

        DebugRenderer = new(Client);
        DependsOn(DebugRenderer);

        GuiRenderer = new(Client);
        DependsOn(GuiRenderer);

        BlitRenderer = new(Client);
        DependsOn(BlitRenderer);

        ImGuiRenderDispatcher = new(Client);
        DependsOn(ImGuiRenderDispatcher);

        FrameBufferTask = PackManager.RegisterResourceLoader(AssetType.Assets, ReloadFrameBuffer);

        ReloadTask = PackManager.RegisterResourceLoader(AssetType.Assets, async (packs) => {
            await RenderSystem.ShaderManager.ReloadTask;
            await FrameBufferTask;
            Reload(packs, RenderSystem, frameBuffer!);
        });
    }

    public void ReloadFrameBuffer(PackManager packs) {
        frameBuffer?.Dispose();
        frameBuffer = new(
            ResourceFactory,
            RenderSystem.GraphicsDevice.MainSwapchain.Framebuffer,
            (uint)Client.nativeWindow!.Width,
            (uint)Client.nativeWindow.Height,
            msaaLevel
        );
    }

    public override void PreRender(double delta) {
        if (needMainBufferRefresh || frameBuffer == null) {
            needMainBufferRefresh = false;
            
            frameBuffer?.Dispose();
            frameBuffer = new(
                ResourceFactory,
                RenderSystem.GraphicsDevice.MainSwapchain.Framebuffer,
                (uint)Client.nativeWindow!.Width,
                (uint)Client.nativeWindow.Height,
                msaaLevel
            );

            RecreatePipelines(frameBuffer);
        }
        CommandList.SetFramebuffer(frameBuffer!.Framebuffer);
        CommandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
        CommandList.ClearDepthStencil(1);
        base.PreRender(delta);
    }

    public void UpdateCamera() {
        MainCamera.position = Client.playerEntity?.SmoothPosition(Client.smoothFactor) + Client.playerEntity?.eyeOffset ?? dvec3.Zero;
        MainCamera.rotationVec = Client.playerEntity?.rotation ?? dvec2.Zero;
        CameraStateManager.SetToCamera(MainCamera);
    }


    public void RecreateMainFramebuffer() {
        needMainBufferRefresh = true;
    }

    public void SetMSAA(uint value) {
        msaaLevel = value;
        RecreateMainFramebuffer();
    }

    public override void Dispose() {
        base.Dispose();

        frameBuffer?.Dispose();
    }
}
