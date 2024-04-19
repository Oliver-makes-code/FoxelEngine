using GlmSharp;
using ImGuiNET;
using Veldrid;
using Voxel.Client.Gui;
using Voxel.Client.Keybinding;
using Voxel.Client.Rendering.Debug;
using Voxel.Client.Rendering.Gui;
using Voxel.Client.Rendering.World;
using Voxel.Common.Util;
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

    public MainFramebuffer? frameBuffer { get; private set; }

    private uint msaaLevel = (uint)ClientConfig.General.msaaLevel;
    private bool needMainBufferRefresh = true;
    

    public GameRenderer(VoxelClient client) : base(client) {
        //Jank but OK
        client.gameRenderer = this;

        MainCamera = new();
        CameraStateManager = new(client.RenderSystem);

        WorldRenderer = new(client);
        DependsOn(WorldRenderer);

        GuiRenderer = new(client);
        DependsOn(GuiRenderer);

        DebugRenderer = new(client);
        DependsOn(DebugRenderer);

        BlitRenderer = new(client);
        DependsOn(BlitRenderer);

        ImGuiRenderDispatcher = new(client);
        DependsOn(ImGuiRenderDispatcher);

        ReloadTask = PackManager.RegisterResourceLoader((packs) => Reload(packs, Client.RenderSystem, null!));
    }

    public override void Reload(PackManager packs, RenderSystem renderSystem, MainFramebuffer _) {
        frameBuffer?.Dispose();
        frameBuffer = new(
            ResourceFactory,
            RenderSystem.GraphicsDevice.MainSwapchain.Framebuffer,
            (uint)Client.NativeWindow.Width,
            (uint)Client.NativeWindow.Height,
            msaaLevel
        );
        base.Reload(packs, renderSystem, frameBuffer);
    }

    public override Pipeline? CreatePipeline(PackManager packs, MainFramebuffer framebuffer) {
        return null;
    }

    public override void PreRender(double delta) {
        if (needMainBufferRefresh || frameBuffer == null) {
            needMainBufferRefresh = false;
            
            frameBuffer?.Dispose();
            frameBuffer = new(
                ResourceFactory,
                RenderSystem.GraphicsDevice.MainSwapchain.Framebuffer,
                (uint)Client.NativeWindow.Width,
                (uint)Client.NativeWindow.Height,
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
        MainCamera.position = Client.PlayerEntity?.SmoothPosition(Client.smoothFactor) + Client.PlayerEntity?.eyeOffset ?? dvec3.Zero;
        MainCamera.rotationVec = Client.PlayerEntity?.rotation ?? dvec2.Zero;
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
