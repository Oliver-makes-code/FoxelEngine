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


    public GameRenderer(VoxelClient client) : base(client) {
        //Jank but OK
        client.GameRenderer = this;

        MainCamera = new();
        CameraStateManager = new(client.RenderSystem);

        WorldRenderer = new(client);
        GuiRenderer = new(client);

        BlitRenderer = new(client);
        DebugRenderer = new(client);
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
        MainCamera.rotationVec = Client.PlayerEntity?.rotation ?? dvec2.Zero;
        CameraStateManager.SetToCamera(MainCamera, Client.timeSinceLastTick);

        WorldRenderer.Render(delta);
        GuiRenderer.Render(delta);

        DebugRenderer.Render(delta);

        Framebuffer.Resolve(RenderSystem);

        BlitRenderer.Blit(Framebuffer.ResolvedMainColor, RenderSystem.GraphicsDevice.MainSwapchain.Framebuffer, true);

        ImGui.Begin("General Debug");
        ImGui.Text($"Player Position: {(Client.PlayerEntity?.blockPosition ?? ivec3.Zero)}");
        ImGui.Text($"Player Velocity: {(Client.PlayerEntity?.velocity.WorldToBlockPosition() ?? ivec3.Zero)}");
        ImGui.Text($"Player Grounded: {Client.PlayerEntity?.isOnFloor ?? false}");
        ImGui.End();

        ImGui.Begin("Input State");

        ImGui.Text("Keybindings");

        ImGui.BeginTable("bindings", 6);
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.TableHeader("Name");
        ImGui.TableSetColumnIndex(1);
        ImGui.TableHeader($"Is Pressed");
        ImGui.TableSetColumnIndex(2);
        ImGui.TableHeader($"Just Pressed");
        ImGui.TableSetColumnIndex(3);
        ImGui.TableHeader($"Just Released");
        ImGui.TableSetColumnIndex(4);
        ImGui.TableHeader($"Strength");
        ImGui.TableSetColumnIndex(5);
        ImGui.TableHeader($"Axis");
        foreach (var (name, bind) in Keybinds.Keybindings) {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(name);
            ImGui.TableSetColumnIndex(1);
            ImGui.Text($"{bind.isPressed}");
            ImGui.TableSetColumnIndex(2);
            ImGui.Text($"{bind.justPressed}");
            ImGui.TableSetColumnIndex(3);
            ImGui.Text($"{bind.justReleased}");
            ImGui.TableSetColumnIndex(4);
            ImGui.Text($"{bind.strength}");
            ImGui.TableSetColumnIndex(5);
            ImGui.Text($"{bind.axis}");
        }
        ImGui.EndTable();

        ImGui.Text("");
        
        ImGui.Text("Connected Gamepads");
        ImGui.BeginTable("gamepad", 2);
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.TableHeader("Index");
        ImGui.TableSetColumnIndex(1);
        ImGui.TableHeader($"Name");
        var gamepads = Client.InputManager.GetRawGamepads();
        foreach (var gamepad in gamepads) {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text($"{gamepad.Index}");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text(gamepad.ControllerName);
        }
        ImGui.EndTable();

        ImGui.End();
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
