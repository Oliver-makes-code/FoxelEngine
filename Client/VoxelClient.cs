using System;
using GlmSharp;
using Voxel.Client.Keybinding;
using Voxel.Client.Network;
using Voxel.Client.Rendering;
using Voxel.Client.Rendering.Debug;
using Voxel.Client.Server;
using Voxel.Client.World;
using Voxel.Client.World.Entity;
using Voxel.Common.Collision;
using Voxel.Common.Util;
using Voxel.Common.World.Entity;
using Voxel.Core;
using Voxel.Core.Util;

namespace Voxel.Client;

public class VoxelClient : Game {
    public static VoxelClient Instance { get; private set; }
    public GameRenderer GameRenderer { get; set; }

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

    public PlayerEntity? PlayerEntity { get; private set; }

    public double timeSinceLastTick;

    public float smoothFactor => (float)(timeSinceLastTick / Constants.SecondsPerTick);

    //public Raycast.HitResult? targetedBlock;


    private dvec3 rayOrigin;
    private dvec3 rayDir;

    public VoxelClient() {
        Instance = this;
    }

    public override void Init() {
        ClientConfig.Load();
        ClientConfig.Save();

        integratedServer = new IntegratedServer();
        integratedServer.Start();
        integratedServer.InternetHostManager.Open();

        connection = new ClientConnectionContext(this, new InternetC2SConnection("localhost"));

        GameRenderer = new(this);
        GameRenderer.MainCamera.aspect = (float)NativeWindow.Width / NativeWindow.Height;
    }

    public void SetupWorld() {
        Console.WriteLine("Client:Setup world!");

        world?.Dispose();
        world = new ClientWorld();

        PlayerEntity = new ControlledClientPlayerEntity();
        world.AddEntity(PlayerEntity, new dvec3(0, 15, 0), dvec2.Zero);
    }

    public override void OnFrame(double delta, double tickAccumulator) {
        timeSinceLastTick = tickAccumulator;
        GameRenderer.Render(delta);

        ImGuiNET.ImGui.ShowMetricsWindow();
    }

    public override void OnTick() {
        Keybinds.Poll();

        if (Keybinds.Pause.justPressed)
            GameRenderer.WorldRenderer.ChunkRenderer.Reload();
        connection?.Tick();
        world?.Tick();
    }

    public override void OnWindowResize() {
        base.OnWindowResize();

        GameRenderer.MainCamera.aspect = (float)NativeWindow.Width / NativeWindow.Height;
    }

    public override void Dispose() {
        GameRenderer.Dispose();
        base.Dispose();
    }
}
