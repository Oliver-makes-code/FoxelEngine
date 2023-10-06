using ImGuiNET;
using RenderSurface.Assets;
using RenderSurface.Input;
using RenderSurface.Rendering;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace RenderSurface;

public abstract class Game : IDisposable {
    public Sdl2Window nativeWindow { get; private set; }
    public GraphicsDevice graphicsDevice { get; private set; }
    public RenderSystem renderSystem { get; private set; }

    public InputManager inputManager { get; private set; }

    public ImGuiRenderer imGuiRenderer { get; private set; }

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
            SyncToVerticalBlank = true,
        };

        VeldridStartup.CreateWindowAndGraphicsDevice(wci, gdo, GraphicsBackend.OpenGL, out var nw, out var gd);
        nativeWindow = nw;
        graphicsDevice = gd;

        isOpen = true;

        var reader = new AssetReader("Content.zip");

        imGuiRenderer = new(gd, gd.SwapchainFramebuffer.OutputDescription, nativeWindow.Width, nativeWindow.Height);
        renderSystem = new(this, reader);

        inputManager = new(this);

        Init();

        double tickFrequency = 1d / tps;
        var lastTime = DateTime.Now;

        while (isOpen && nativeWindow.Exists) {
            var newTime = DateTime.Now;
            double difference = (newTime - lastTime).TotalSeconds;
            lastTime = newTime;

            tickAccumulator += difference;
            for (int i = 0; i < 3 && tickAccumulator > tickFrequency; i++) {
                tickAccumulator -= tickFrequency;

                OnTick();
            }

            var inputState = nativeWindow.PumpEvents();
            imGuiRenderer.Update((float)difference, inputState);

            //TODO - Remove
            ImGui.ShowDemoWindow();

            renderSystem.StartFrame(difference);
            OnFrame(difference);
            renderSystem.EndFrame();
        }

        isOpen = false;
    }

    public abstract void Init();
    public abstract void OnFrame(double delta);
    public abstract void OnTick();

    public virtual void Dispose() {
        graphicsDevice?.Dispose();
    }
}
