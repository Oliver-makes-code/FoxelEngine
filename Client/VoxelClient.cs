using GlmSharp;
using Veldrid.Sdl2;
using Voxel.Client.Keybinding;
using Voxel.Client.Network;
using Voxel.Client.Rendering;
using Voxel.Client.Rendering.Debug;
using Voxel.Client.Server;
using Voxel.Client.World;
using Voxel.Client.World.Entity;
using Voxel.Client.World.Gui;
using Voxel.Client.World.Gui.Render;
using Voxel.Common.Collision;
using Voxel.Common.Util;
using Voxel.Core.Util.Profiling;
using Voxel.Core;
using System;

namespace Voxel.Client;

public class VoxelClient : Game {

    public static VoxelClient? instance { get; private set; }

    public static bool isMouseCapruted;

    private static readonly Profiler.ProfilerKey UpdateFrame = Profiler.GetProfilerKey("Update Frame");

    public GameRenderer? gameRenderer { get; set; }

    /// <summary>
    /// Instance of integrated server, if there is any currently loaded.
    /// </summary>
    public IntegratedServer? integratedServer { get; private set; }
    

    /// <summary>
    /// Connection object that's used to communicate with whatever server we're currently connected to.
    /// </summary>
    public ClientConnectionContext? connection { get; private set; }

    /// <summary>
    /// Client instance of the world that's loaded on the client. All the communication the server does about the world goes into here.
    /// </summary>
    public ClientWorld? world { get; private set; }

    public ClientGuiScreen? screen { get; private set; }
    
    public ControlledClientPlayerEntity? playerEntity { get; internal set; }

    public double timeSinceLastTick;

    public float smoothFactor => (float)(timeSinceLastTick / Constants.SecondsPerTick);

    public VoxelClient() {
        instance = this;
    }

    private static void CaptureMouse(bool captured) {
        if (Sdl2Native.SDL_SetRelativeMouseMode(captured) == -1)
            return;
        isMouseCapruted = captured;
    }

    public override void Init() {
        // SAFETY: We're only passing a single pointer that we know is non null.
        unsafe {
            SDL_version v;
            Sdl2Native.SDL_GetVersion(&v);
            Logger.Info($"SDL Version: {v.major}.{v.minor}.{v.patch}");
        }
        
        // DiscordRpcManager.Initialize();
        // DiscordRpcManager.UpdateStatus("test", "nya :3");

        integratedServer = new();
        integratedServer.Start();
        integratedServer.InternetHostManager.Open();

        connection = new(this, new InternetC2SConnection("localhost"));
        
        gameRenderer = new(this);
        gameRenderer.MainCamera.aspect = (float)nativeWindow!.Width / nativeWindow.Height;

        GuiScreenRendererRegistry.Register<PlayerHudScreen>((s) => new PlayerHudGuiScreenRenderer(s));
    }

    public void SetupWorld() {
        Logger.Info("Setup world!");

        world?.Dispose();
        world = new();
    }
    
    public override void OnFrame(double delta, double tickAccumulator) {
        if (gameRenderer == null)
            return;
        
        if (screen == null && playerEntity != null) {
            screen = new PlayerHudScreen(playerEntity!);
            screen.Open();
        }

        Keybinds.Poll();

        using (UpdateFrame.Push()) {
            if (Keybinds.Refresh.justPressed)
                ReloadPacks();

            if (Keybinds.Pause.justPressed)
                CaptureMouse(!isMouseCapruted);

            if (isMouseCapruted)
                nativeWindow!.SetMousePosition(new(nativeWindow.Width/2, nativeWindow.Height/2));

            playerEntity?.Update(delta);

            timeSinceLastTick = tickAccumulator;

            gameRenderer.UpdateCamera();

            if (playerEntity != null) {
                var pos = gameRenderer.MainCamera.position;
                var rot = quat.Identity
                    .Rotated((float)playerEntity.rotation.y, new(0, 1, 0))
                    .Rotated((float)playerEntity.rotation.x, new(1, 0, 0));
                var projected = rot * new vec3(0, 0, -5);

                if (world!.Raycast(new RaySegment(new Ray(pos, projected), 5), out var hit))
                    DebugRenderer.DrawCube(hit.blockPos, hit.blockPos + 1, 0.001f);
            }
        }
        
        gameRenderer.PreRender(delta);
        gameRenderer.PostRender(delta);
    }

    public override void OnTick() {
        connection?.Tick();
        world?.Tick();
        screen?.Tick();
    }

    public override void OnWindowResize() {
        base.OnWindowResize();

        if (gameRenderer == null)
            return;

        gameRenderer.MainCamera.aspect = (float)nativeWindow!.Width / nativeWindow.Height;
        gameRenderer.RecreateMainFramebuffer();
    }

    public override void Dispose() {
        gameRenderer?.Dispose();
        base.Dispose();
    }
}
