using GlmSharp;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Vortice.Mathematics;
using Voxel.Common.Util.Profiling;
using Voxel.Core.Assets;
using Voxel.Core.Input;
using Voxel.Core.Rendering;
using MathHelper = Voxel.Core.Util.MathHelper;

namespace Voxel.Core;

public abstract class Game : IDisposable {

    private static readonly Profiler.ProfilerKey FrameKey = Profiler.GetProfilerKey("Frame");
    private static readonly Profiler.ProfilerKey TickKey = Profiler.GetProfilerKey("Tick");

    public readonly PackManager PackManager = new(AssetType.Assets);

    public Sdl2Window NativeWindow { get; private set; }
    public GraphicsDevice GraphicsDevice { get; private set; }
    public RenderSystem RenderSystem { get; private set; }

    public InputManager InputManager { get; private set; }

    public ImGuiRenderer ImGuiRenderer { get; private set; }

    public AssetReader AssetReader { get; private set; }

    public bool isOpen { get; private set; }

    private double tickAccumulator;

    public void Run(int tps = 20, string windowTitle = "Game") {
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
            SyncToVerticalBlank = false,
        };

        VeldridStartup.CreateWindowAndGraphicsDevice(wci, gdo, GraphicsBackend.Vulkan, out var nw, out var gd);
        NativeWindow = nw;
        GraphicsDevice = gd;

        isOpen = true;

        AssetReader = new("Content.zip");

        ImGuiRenderer = new(gd, gd.SwapchainFramebuffer.OutputDescription, NativeWindow.Width, NativeWindow.Height);
        RenderSystem = new(this, AssetReader, PackManager);

        Sdl2Native.SDL_Init(SDLInitFlags.Joystick | SDLInitFlags.GameController);

        InputManager = new(this);

        NativeWindow.Resized += () => {
            ImGuiRenderer.WindowResized(NativeWindow.Width, NativeWindow.Height);
            OnWindowResize();
        };

        Init();

        PackManager.ReloadPacks().Wait();

        double tickFrequency = 1d / tps;
        var lastTime = DateTime.Now;

        bool windowClosed = false;

        NativeWindow.Closed += () => windowClosed = true;

        while (isOpen && NativeWindow.Exists && !windowClosed) {
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

            var inputState = NativeWindow.PumpEvents();
            if (windowClosed)
                break;
            Profiler.Init("Client Frame");

            using (FrameKey.Push()) {
                ImGuiRenderer.Update((float)difference, inputState);

                OnFrame(difference, tickAccumulator);

                RenderSystem.MainCommandList.SetFramebuffer(RenderSystem.GraphicsDevice.SwapchainFramebuffer);
                ImGuiRenderer.Render(GraphicsDevice, RenderSystem.MainCommandList);
                RenderSystem.EndFrame();
            }
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
}
