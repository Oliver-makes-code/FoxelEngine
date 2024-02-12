using GlmSharp;
using ImGuiNET;
using Veldrid;
using Voxel.Client.Gui;
using Voxel.Client.Keybinding;
using Voxel.Client.Rendering.Debug;
using Voxel.Client.Rendering.Gui;
using Voxel.Client.Rendering.World;
using Voxel.Common.Util;

namespace Voxel.Client.Rendering;

public class GameRenderer : Renderer {

    public MainFramebuffer Framebuffer { get; private set; }
    private uint msaaLevel = (uint)ClientConfig.General.msaaLevel;
    private bool needMainBufferRefresh = true;

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
    

    public GameRenderer(VoxelClient client) : base(client) {
        //Jank but OK
        client.GameRenderer = this;

        MainCamera = new();
        CameraStateManager = new(client.RenderSystem);

        WorldRenderer = new(client);
        GuiRenderer = new(client);

        BlitRenderer = new(client);
        DebugRenderer = new(client);
        ImGuiRenderDispatcher = new ImGuiRenderDispatcher(client);
    }

    public override void CreatePipeline(MainFramebuffer framebuffer) {
        WorldRenderer.CreatePipeline(framebuffer);
        GuiRenderer.CreatePipeline(framebuffer);

        BlitRenderer.CreatePipeline(framebuffer);
        DebugRenderer.CreatePipeline(framebuffer);
    }
    public override void Render(double delta) {

        if (needMainBufferRefresh) {
            needMainBufferRefresh = false;

            Framebuffer?.Dispose();
            Framebuffer = new MainFramebuffer(ResourceFactory, RenderSystem.GraphicsDevice.MainSwapchain.Framebuffer, (uint)Client.NativeWindow.Width, (uint)Client.NativeWindow.Height, msaaLevel);

            CreatePipeline(Framebuffer);
        }

        CommandList.SetFramebuffer(Framebuffer.Framebuffer);
        CommandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
        //CommandList.ClearColorTarget(1, RgbaFloat.Green);
        CommandList.ClearDepthStencil(1);

        WorldRenderer.Render(delta);
        GuiRenderer.Render(delta);
        
        DebugRenderer.Render(delta);

        Framebuffer.Resolve(RenderSystem);

        BlitRenderer.Blit(Framebuffer.ResolvedMainColor, RenderSystem.GraphicsDevice.MainSwapchain.Framebuffer, true);
        
        ImGuiRenderDispatcher.Render(delta);
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
        WorldRenderer.Dispose();
        GuiRenderer.Dispose();

        BlitRenderer.Dispose();
        DebugRenderer.Dispose();

        Framebuffer.Dispose();
    }
}
