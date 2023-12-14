using GlmSharp;
using ImGuiNET;
using Veldrid;
using Voxel.Client.Rendering.Debug;
using Voxel.Client.Rendering.GUI;
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
    public readonly GUIRenderer GUIRenderer;

    public readonly BlitRenderer BlitRenderer;
    public readonly DebugRenderer DebugRenderer;


    public GameRenderer(VoxelClient client) : base(client) {
        //Jank but OK
        client.GameRenderer = this;

        MainCamera = new();
        CameraStateManager = new(client.RenderSystem);

        WorldRenderer = new(client);
        GUIRenderer = new(client);

        BlitRenderer = new(client);
        DebugRenderer = new(client);
    }

    public override void CreatePipeline(MainFramebuffer framebuffer) {
        WorldRenderer.CreatePipeline(framebuffer);
        GUIRenderer.CreatePipeline(framebuffer);

        BlitRenderer.CreatePipeline(framebuffer);
        DebugRenderer.CreatePipeline(framebuffer);
    }
    public override void Render(double delta) {

        if (needMainBufferRefresh) {
            needMainBufferRefresh = false;

            if (Framebuffer != null)
                Framebuffer.Dispose();
            Framebuffer = new MainFramebuffer(ResourceFactory, RenderSystem.GraphicsDevice.MainSwapchain.Framebuffer, (uint)Client.NativeWindow.Width, (uint)Client.NativeWindow.Height, msaaLevel);

            CreatePipeline(Framebuffer);
        }

        CommandList.SetFramebuffer(Framebuffer.Framebuffer);
        CommandList.ClearColorTarget(0, RgbaFloat.Grey);
        CommandList.ClearColorTarget(1, RgbaFloat.Green);
        CommandList.ClearDepthStencil(1);

        MainCamera.position = Client.PlayerEntity?.SmoothPosition(Client.smoothFactor) + Client.PlayerEntity?.eyeOffset ?? dvec3.Zero;
        MainCamera.rotationVec = Client.PlayerEntity?.SmoothRotation(Client.smoothFactor) ?? dvec2.Zero;
        CameraStateManager.SetToCamera(MainCamera, Client.timeSinceLastTick);

        WorldRenderer.Render(delta);
        GUIRenderer.Render(delta);

        DebugRenderer.Render(delta);

        Framebuffer.Resolve(RenderSystem);

        BlitRenderer.Blit(Framebuffer.ResolvedMainColor, RenderSystem.GraphicsDevice.MainSwapchain.Framebuffer, true);


        ImGui.Text($"Player Position: {(Client.PlayerEntity?.blockPosition ?? ivec3.Zero)}");
        ImGui.Text($"Player Velocity: {(Client.PlayerEntity?.velocity.WorldToBlockPosition() ?? ivec3.Zero)}");
        ImGui.Text($"Player Grounded: {Client.PlayerEntity?.isOnFloor ?? false}");
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
        GUIRenderer.Dispose();

        BlitRenderer.Dispose();
        DebugRenderer.Dispose();

        Framebuffer.Dispose();
    }
}
