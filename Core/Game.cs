using GlmSharp;
using NLog;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Vortice.Mathematics;
using Voxel.Core.Util.Profiling;
using Voxel.Core.Assets;
using Voxel.Core.Input;
using Voxel.Core.Rendering;
using MathHelper = Voxel.Core.Util.MathHelper;
using Microsoft.CodeAnalysis.Scripting;
using Voxel.Core.Util;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis;

namespace Voxel.Core;

public abstract class Game : IDisposable {
    public static readonly Logger Logger = LogManager.GetLogger("Client");

    private static readonly Profiler.ProfilerKey FrameKey = Profiler.GetProfilerKey("Frame");
    private static readonly Profiler.ProfilerKey TickKey = Profiler.GetProfilerKey("Tick");

    public readonly PackManager PackManager = new(AssetType.Assets);

    public Sdl2Window? nativeWindow { get; private set; }
    public GraphicsDevice? graphicsDevice { get; private set; }
    public RenderSystem? renderSystem { get; private set; }

    public InputManager? inputManager { get; private set; }

    public ImGuiRenderer? imGuiRenderer { get; private set; }

    public bool isOpen { get; private set; }

    public ivec2 screenSize => nativeWindow == null ? new(0) : new(nativeWindow.Width, nativeWindow.Height);

    private double tickAccumulator;

    public async Task Run(int tps = 20, string windowTitle = "Game") {
        LoggerConfig.Init();

        try {
            var wci = new WindowCreateInfo {
                X = 100,
                Y = 100,
                WindowWidth = 1280,
                WindowHeight = 720,
                WindowTitle = windowTitle,
            };

            var gdo = new GraphicsDeviceOptions {
                PreferDepthRangeZeroToOne = true,
                PreferStandardClipSpaceYDirection = true,
                SyncToVerticalBlank = true,
            };

            VeldridStartup.CreateWindowAndGraphicsDevice(wci, gdo, GraphicsBackend.Vulkan, out var nw, out var gd);
            nativeWindow = nw;
            graphicsDevice = gd;

            isOpen = true;

            imGuiRenderer = new(gd, gd.SwapchainFramebuffer.OutputDescription, nativeWindow.Width, nativeWindow.Height);
            renderSystem = new(this, PackManager);

            Sdl2Native.SDL_Init(SDLInitFlags.Joystick | SDLInitFlags.GameController);

            inputManager = new(this);

            nativeWindow.Resized += () => {
                imGuiRenderer.WindowResized(nativeWindow.Width, nativeWindow.Height);
                OnWindowResize();
            };

            Init();

            await ReloadPacksAsync();

            double tickFrequency = 1d / tps;
            var lastTime = DateTime.Now;

            bool windowClosed = false;

            nativeWindow.Closed += () => windowClosed = true;

            while (isOpen && nativeWindow.Exists && !windowClosed) {
                var newTime = DateTime.Now;
                double difference = (newTime - lastTime).TotalSeconds;
                lastTime = newTime;

                tickAccumulator += difference;
                if (tickAccumulator > tickFrequency) {
                    tickAccumulator -= tickFrequency;

                    Profiler.Init("Client Tick");
                    
                    using (TickKey.Push()) {
                        OnTick();
                    }
                }

                tickAccumulator = MathHelper.Repeat(tickAccumulator, tickFrequency);

                var inputState = nativeWindow.PumpEvents();
                if (windowClosed)
                    break;
                Profiler.Init("Client Frame");

                using (FrameKey.Push()) {
                    imGuiRenderer.Update((float)difference, inputState);

                    OnFrame(difference, tickAccumulator);

                    renderSystem.MainCommandList.SetFramebuffer(renderSystem.GraphicsDevice.SwapchainFramebuffer);
                    imGuiRenderer.Render(graphicsDevice, renderSystem.MainCommandList);
                    lock (renderSystem) {
                        renderSystem.EndFrame();
                    }
                }
            }
        } catch (Exception e) {
            Logger.Fatal(e);
        }

        isOpen = false;
    }

    public abstract void Init();
    public abstract void OnFrame(double delta, double tickAccumulator);
    public abstract void OnTick();

    public virtual void OnWindowResize() {

    }

    public virtual void Dispose() {
        // This is causing a hang-up when exiting. TODO: investigate
        // GraphicsDevice.Dispose();
    }

    public Task ReloadPacksAsync()
        => PackManager.ReloadPacks();

    public void ReloadPacks()
        => PackManager.ReloadPacks().Wait();
}
