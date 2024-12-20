using GlmSharp;
using Foxel.Client.Network;
using Foxel.Client.Rendering;
using Foxel.Client.Rendering.Debug;
using Foxel.Client.Server;
using Foxel.Client.World;
using Foxel.Client.World.Content.Entities;
using Foxel.Client.World.Gui;
using Foxel.Client.World.Gui.Render;
using Foxel.Common.Collision;
using Foxel.Common.Util;
using Foxel.Core.Util.Profiling;
using Foxel.Core;
using System;
using Foxel.Client.Input;
using System.Threading.Tasks;
using Foxel.Client.Rendering.Texture;
using Foxel.Common.World.Content;
using System.Collections.Concurrent;
using System.Threading;

namespace Foxel.Client;

public class VoxelClient : Game {
    private static readonly ConcurrentQueue<Action<VoxelClient>> ThreadWorkQueue = [];

    public static VoxelClient? instance { get; private set; }

    private static Thread? clientThread;

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

    public ModelTextureizer? modelTextureizer;

    public VoxelClient() {
        instance = this;
        clientThread = Thread.CurrentThread;
    }

    public void RunOnClientThread(Action<VoxelClient> action) {
        if (Thread.CurrentThread != clientThread)
            ThreadWorkQueue.Enqueue(action);
        action(this);
    }

    public override async Task Init() {
        ContentStores.InitStaticStores();
        
        // DiscordRpcManager.Initialize();
        // DiscordRpcManager.UpdateStatus("test", "nya :3");

        integratedServer = new();
        await integratedServer.Start();
        integratedServer.InternetHostManager.Open();

        connection = new(this, new InternetC2SConnection("localhost"));
        
        gameRenderer = new(this);
        gameRenderer.MainCamera.aspect = (float)nativeWindow!.Width / nativeWindow.Height;

        GuiScreenRendererRegistry.Register<PlayerHudScreen>((s) => new PlayerHudGuiScreenRenderer(s));

        modelTextureizer = new(this);
    }

    public void SetupWorld() {
        Logger.Info("Setup world!");

        world?.Dispose();
        world = new();
    }
    
    public override void OnFrame(double delta, double tickAccumulator) {
        if (gameRenderer == null)
            return;

        gameRenderer.TrySaveScreenshot();
        
        if (screen == null && playerEntity != null) {
            screen = new PlayerHudScreen(playerEntity!);
            screen.Open();
        }

        ActionGroup.UpdateAll(inputManager!);

        using (UpdateFrame.Push()) {
            if (ActionGroups.Refresh.WasJustPressed())
                ReloadPacks();

            if (ActionGroups.Pause.WasJustPressed())
                CaptureMouse(!isMouseCapruted);

            if (ActionGroups.Screenshot.WasJustPressed())
                gameRenderer.MarkForScreenshot();

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

                if (world!.Raycast(new RaySegment(new Ray(pos, projected), 5), out var hit)) {
                    var state = world!.GetBlockState(hit.blockPos);
                    foreach (var box in state.Block.GetShape(state).LocalBoxes(hit.blockPos)) {
                        DebugRenderer.DrawCube(box.min, box.max, 0.01f);
                    }
                }
            }

            while (ThreadWorkQueue.TryDequeue(out var action))
                action(this);
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
