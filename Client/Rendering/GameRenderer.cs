using System;
using GlmSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Foxel.Client.Rendering.Debug;
using Foxel.Client.Rendering.Gui;
using Foxel.Client.Rendering.World;
using Foxel.Core;
using Foxel.Core.Assets;
using Foxel.Core.Rendering;

namespace Foxel.Client.Rendering;

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

    private bool shouldScreenshot = false;
    

    public GameRenderer(VoxelClient client) : base(client) {
        //Jank but OK
        client.gameRenderer = this;

        MainCamera = new();
        CameraStateManager = new(RenderSystem);

        WorldRenderer = new(Client);
        DependsOn(WorldRenderer);

        DebugRenderer = new(Client);
        DependsOn(DebugRenderer);

        BlitRenderer = new(Client);
        DependsOn(BlitRenderer);

        GuiRenderer = new(Client);
        DependsOn(GuiRenderer);

        ImGuiRenderDispatcher = new(Client);
        DependsOn(ImGuiRenderDispatcher);

        FrameBufferTask = PackManager.RegisterResourceLoader(AssetType.Assets, ReloadFrameBuffer);

        ReloadTask = PackManager.RegisterResourceLoader(AssetType.Assets, async (packs) => {
            await RenderSystem.ShaderManager.ReloadTask;
            await FrameBufferTask;
            Reload(packs, RenderSystem, frameBuffer!);
        });
    }

    public void MarkForScreenshot() {
        shouldScreenshot = true;
    }

    public void TrySaveScreenshot() {
        if (frameBuffer == null)
            return;
        if (!shouldScreenshot)
            return;
        shouldScreenshot = false;
        var mappedImage = RenderSystem.GraphicsDevice.Map<hvec4>(frameBuffer.Staging, MapMode.Read);
        var arr = new RgbaVector[mappedImage.Count];
        for (int i = 0; i < mappedImage.Count; i++) {
            var pixel = (vec4)mappedImage[i];
            arr[i] = new(pixel.r, pixel.g, pixel.b);
        }
        var image = Image.LoadPixelData<RgbaVector>(arr.AsSpan(), (int)frameBuffer.Staging.Width, (int)frameBuffer.Staging.Height);
        image.SaveAsPng("screenshot.png");
        Game.Logger.Info("Screenshot saved as screenshot.png");
    }

    public void ReloadFrameBuffer(PackManager packs) {
        frameBuffer?.Dispose();
        frameBuffer = new(
            RenderSystem.TextureManager,
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
                RenderSystem.TextureManager,
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
        CommandList.ClearColorTarget(1, RgbaFloat.Clear);
        CommandList.ClearDepthStencil(1);
        base.PreRender(delta);
    }

    public override void PostRender(double delta) {
        base.PostRender(delta);
        if (shouldScreenshot)
            CommandList.CopyTexture(frameBuffer!.Framebuffer.ColorTargets[0].Target, frameBuffer!.Staging);
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
