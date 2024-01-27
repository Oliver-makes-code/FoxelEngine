using System;
using System.Runtime.InteropServices;
using GlmSharp;
using Veldrid.Sdl2;
using Voxel.Client.Gui;
using Voxel.Client.Keybinding;
using Voxel.Client.Network;
using Voxel.Client.Rendering;
using Voxel.Client.Server;
using Voxel.Client.Social.Discord;
using Voxel.Client.World;
using Voxel.Client.World.Entity;
using Voxel.Common.Util;
using Voxel.Common.Util.Profiling;
using Voxel.Core;

namespace Voxel.Client;

public class VoxelClient : Game {

    public static VoxelClient Instance { get; private set; }

    public static bool isMouseCapruted;
    public static bool justCapturedMouse;

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

    public ControlledClientPlayerEntity? PlayerEntity { get; internal set; }

    public double timeSinceLastTick;

    public float smoothFactor => (float)(timeSinceLastTick / Constants.SecondsPerTick);

    //public Raycast.HitResult? targetedBlock;


    private dvec3 rayOrigin;
    private dvec3 rayDir;

    private bool useMSAA = false;

    public VoxelClient() {
        Instance = this;
    }

    public override void Init() {
        // SAFETY: We're only passing a single pointer that we know is non null.
        unsafe {
            SDL_version v;
            Sdl2Native.SDL_GetVersion(&v);
            Console.WriteLine($"SDL Version: {v.major}.{v.minor}.{v.patch}");
        }

        ClientConfig.Load();
        ClientConfig.Save();
        
        // DiscordRpcManager.Initialize();
        // DiscordRpcManager.UpdateStatus("test", "nya :3");

        integratedServer = new();
        integratedServer.Start();
        integratedServer.InternetHostManager.Open();

        connection = new(this, new InternetC2SConnection("localhost"));

        GameRenderer = new(this);
        GameRenderer.MainCamera.aspect = (float)NativeWindow.Width / NativeWindow.Height;
        
        new PlayerHUDScreen().BuildClientGui();
    }

    public void SetupWorld() {
        Console.WriteLine("Client:Setup world!");

        world?.Dispose();
        world = new ClientWorld();
    }
    
    public override void OnFrame(double delta, double tickAccumulator) {
        Keybinds.Poll();
        justCapturedMouse = false;

        if (Keybinds.Refresh.justPressed)
            NativeWindow.BorderVisible = !NativeWindow.BorderVisible;

        if (Keybinds.Pause.justPressed)
            CaptureMouse(!isMouseCapruted);

        if (isMouseCapruted)
            NativeWindow.SetMousePosition(new(NativeWindow.Width/2, NativeWindow.Height/2));

        PlayerEntity?.Update(delta);

        timeSinceLastTick = tickAccumulator;
        
        GameRenderer.Render(delta);
    }

    public override void OnTick() {
        connection?.Tick();
        world?.Tick();
    }

    public override void OnWindowResize() {
        base.OnWindowResize();

        GameRenderer.MainCamera.aspect = (float)NativeWindow.Width / NativeWindow.Height;
        GameRenderer.RecreateMainFramebuffer();
    }

    public override void Dispose() {
        GameRenderer.Dispose();
        base.Dispose();
    }

    private static void CaptureMouse(bool captured) {
        if (Sdl2Native.SDL_SetRelativeMouseMode(captured) == -1)
            return;
        if (captured && !isMouseCapruted)
            justCapturedMouse = true;
        isMouseCapruted = captured;
    }
}
