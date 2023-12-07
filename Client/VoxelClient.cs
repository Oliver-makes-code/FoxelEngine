using System;
using GlmSharp;
using RenderSurface;
using Voxel.Client.Keybinding;
using Voxel.Client.Network;
using Voxel.Client.Rendering;
using Voxel.Client.Server;
using Voxel.Client.World;
using Voxel.Common.Entity;
using Voxel.Common.World;

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

    public Raycast.HitResult? targetedBlock;

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

        PlayerEntity = new PlayerEntity();
        world.AddEntity(PlayerEntity, dvec3.Zero, 0);
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
