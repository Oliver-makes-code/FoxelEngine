using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Voxel.Client;

public static class VoxelClient {
    public static IWindow? Win { get; private set; }
    public static IInputContext? Input { get; private set; }
    public static DrawHelper? Helper { get; private set; }

    private static bool IsInit = false;

    public static void Init() {
        if (IsInit)
            return;
        
        IsInit = true;

        WindowOptions options = WindowOptions.Default with {
    		Size = new Vector2D<int>(800, 600),
    		Title = "Voxel Game Engine"
		};

		Win = Window.Create(options);

        Win.Load += WindowInit;

        Win.Update += Update;

        Helper = new(Win);
        
        Win.Render += Helper.Render;
    }

    public static void Run() {
        if (!IsInit)
            return;
        Win!.Run();
    }

    private static void WindowInit() {
        Input = Win!.CreateInput();

        Input.ConnectionChanged += InputChange;

        for (int i = 0; i < Input.Keyboards.Count; i++) {
            Input.Keyboards[i].KeyDown += KeyDown;
            Input.Keyboards[i].KeyUp += KeyUp;
        }

        Helper!.Init();
    }

    private static void InputChange(IInputDevice device, bool connected) {
        Console.WriteLine(device);
        Console.WriteLine(connected);
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode) {
        Keybinds.SendChange(key, true);
    }

    private static void KeyUp(IKeyboard keyboard, Key key, int keyCode) {
        Keybinds.SendChange(key, false);
    }

    private static void Update(double delta) {
        Keybinds.UpdateAll();
    }
}
