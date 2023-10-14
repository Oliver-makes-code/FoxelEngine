using System.Collections.Generic;
using RenderSurface.Input.Gamepad;
using Veldrid;
using VMouseButton = Veldrid.MouseButton;

namespace Voxel.Client.Keybinding;

public static class Keybinds {
    public static readonly Dictionary<string, Keybind> Keybindings = new();

    public static readonly Keybind Pause = new(
        "pause",
        KeyButton.Get(Key.Escape),
        ControllerNewButton.Get(GamepadButton.Start)
    );

    public static readonly Keybind Forward = new(
        "movement.forward",
        KeyButton.Get(Key.W),
        ControllerAxisButton.Get(GamepadAxis.LeftYNegative)
    );

    public static readonly Keybind Backward = new(
        "movement.backward",
        KeyButton.Get(Key.S),
        ControllerAxisButton.Get(GamepadAxis.LeftYPositive)
    );

    public static readonly Keybind StrafeLeft = new(
        "movement.strafe.left",
        KeyButton.Get(Key.A),
        ControllerAxisButton.Get(GamepadAxis.LeftXNegative)
    );

    public static readonly Keybind StrafeRight = new(
        "movement.strafe.right",
        KeyButton.Get(Key.D),
        ControllerAxisButton.Get(GamepadAxis.LeftXPositive)
    );

    public static readonly Keybind Jump = new(
        "movement.jump",
        KeyButton.Get(Key.Space),
        ControllerNewButton.Get(GamepadButton.A)
    );

    public static readonly Keybind Crouch = new(
        "movement.crouch",
        KeyButton.Get(Key.ShiftLeft),
        ControllerNewButton.Get(GamepadButton.RightStick)
    );

    public static readonly Keybind LookUp = new(
        "camera.up",
        KeyButton.Get(Key.Up),
        ControllerAxisButton.Get(GamepadAxis.RightYNegative)
    );

    public static readonly Keybind LookDown = new(
        "camera.down",
        KeyButton.Get(Key.Down),
        ControllerAxisButton.Get(GamepadAxis.RightYPositive)
    );

    public static readonly Keybind LookLeft = new(
        "camera.left",
        KeyButton.Get(Key.Left),
        ControllerAxisButton.Get(GamepadAxis.RightXNegative)
    );

    public static readonly Keybind LookRight = new(
        "camera.right",
        KeyButton.Get(Key.Right),
        ControllerAxisButton.Get(GamepadAxis.RightXPositive)
    );

    public static readonly Keybind Use = new(
        "action.use",
        MouseButton.Get(VMouseButton.Right),
        ControllerAxisButton.Get(GamepadAxis.LeftTrigger)
    );

    public static readonly Keybind Attack = new(
        "action.attack",
        MouseButton.Get(VMouseButton.Left),
        ControllerAxisButton.Get(GamepadAxis.RightTrigger)
    );

    public static readonly Keybind Refresh = new(
        "debug.refresh",
        KeyButton.Get(Key.F3),
        ControllerNewButton.Get(GamepadButton.Start)
    );

    public static void ReadFromConfig() {
        foreach (var bind in ClientConfig.keybindings) {
            if (!Keybindings.ContainsKey(bind.Key))
                continue;
            var bindToSet = Keybindings[bind.Key];
            bindToSet.ReadButtonString(bind.Value);
        }
    }

    public static void WriteToConfig() {
        Dictionary<string, string[]> keybinds = new();

        foreach (var bind in Keybindings)
            keybinds[bind.Key] = bind.Value.GetButtonString();

        ClientConfig.keybindings = keybinds;
    }

    public static void Poll() {
        foreach (var bind in Keybindings.Values)
            bind.Poll();
    }
}
