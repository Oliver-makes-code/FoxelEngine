using ImGuiNET;
using RenderSurface.Assets;
using RenderSurface.Input;
using RenderSurface.Rendering;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace RenderSurface;

public abstract class Game : IDisposable {
    public Sdl2Window NativeWindow { get; private set; }
    public GraphicsDevice GraphicsDevice { get; private set; }
    public RenderSystem RenderSystem { get; private set; }

    public InputManager InputManager { get; private set; }

    public ImGuiRenderer ImGuiRenderer { get; private set; }

    public bool IsOpen { get; private set; }

    private double _tickAccumulator = 0;

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
            SyncToVerticalBlank = true,
            Debug = true
        };

        VeldridStartup.CreateWindowAndGraphicsDevice(wci, gdo, GraphicsBackend.Vulkan, out var nw, out var gd);
        NativeWindow = nw;
        GraphicsDevice = gd;

        IsOpen = true;

        var reader = new AssetReader("Content.zip");

        ImGuiRenderer = new(gd, gd.SwapchainFramebuffer.OutputDescription, NativeWindow.Width, NativeWindow.Height);
        RenderSystem = new(this, reader);

        InputManager = new InputManager(this);

        Init();

        var tickFrequency = 1d / tps;
        var lastTime = DateTime.Now;

        while (IsOpen && NativeWindow.Exists) {
            var newTime = DateTime.Now;
            var difference = (newTime - lastTime).TotalSeconds;
            lastTime = newTime;


            _tickAccumulator += difference;
            for (int i = 0; i < 3 && _tickAccumulator > tickFrequency; i++) {
                _tickAccumulator -= tickFrequency;

                OnTick();
            }

            var inputState = NativeWindow.PumpEvents();
            ImGuiRenderer.Update((float)difference, inputState);

            //TODO - Remove
            ImGui.ShowDemoWindow();

            RenderSystem.StartFrame(difference);
            OnFrame(difference);
            RenderSystem.EndFrame();
        }

        IsOpen = false;
    }

    public abstract void Init();
    public abstract void OnFrame(double delta);
    public abstract void OnTick();

    public virtual void Dispose() {
        GraphicsDevice?.Dispose();
    }
}
