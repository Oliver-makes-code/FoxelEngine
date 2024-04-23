using System.Collections.Generic;
using Veldrid;
using Voxel.Core.Util.Profiling;
using Voxel.Core.Input.Gamepad;
using VMouseButton = Veldrid.MouseButton;

namespace Voxel.Client.Keybinding;

public static class Keybinds {
    public static readonly Dictionary<string, Keybind> Keybindings = [];

    public static readonly Keybind Pause = new(
        "pause",
        KeyButton.Get(Key.Escape),
        ControllerButton.Get(GamepadButton.Start)
    );

    public static readonly Keybind Move = new(
        "movement.full",
        ControllerJoystickButton.Get(ControllerJoystickButton.GamepadJoystick.Left)
    );

    public static readonly Keybind Forward = new(
        "movement.forward",
        KeyButton.Get(Key.W)
    );

    public static readonly Keybind Backward = new(
        "movement.backward",
        KeyButton.Get(Key.S)
    );

    public static readonly Keybind StrafeLeft = new(
        "movement.strafe.left",
        KeyButton.Get(Key.A)
    );

    public static readonly Keybind StrafeRight = new(
        "movement.strafe.right",
        KeyButton.Get(Key.D)
    );

    public static readonly Keybind Jump = new(
        "movement.jump",
        KeyButton.Get(Key.Space),
        ControllerButton.Get(GamepadButton.A)
    );

    public static readonly Keybind Crouch = new(
        "movement.crouch",
        KeyButton.Get(Key.ShiftLeft),
        ControllerButton.Get(GamepadButton.RightStick)
    );

    public static readonly Keybind Look = new(
        "camera.full",
        ControllerJoystickButton.Get(ControllerJoystickButton.GamepadJoystick.Right)
    );

    public static readonly Keybind LookUp = new(
        "camera.up",
        KeyButton.Get(Key.Up)
    );

    public static readonly Keybind LookDown = new(
        "camera.down",
        KeyButton.Get(Key.Down)
    );

    public static readonly Keybind LookLeft = new(
        "camera.left",
        KeyButton.Get(Key.Left)
    );

    public static readonly Keybind LookRight = new(
        "camera.right",
        KeyButton.Get(Key.Right)
    );

    public static readonly Keybind Use = new(
        "action.use",
        MouseButton.Get(VMouseButton.Right),
        ControllerTriggerButton.Get(ControllerTriggerButton.GamepadTrigger.Left)
    );

    public static readonly Keybind Attack = new(
        "action.attack",
        MouseButton.Get(VMouseButton.Left),
        ControllerTriggerButton.Get(ControllerTriggerButton.GamepadTrigger.Right)
    );

    public static readonly Keybind Refresh = new(
        "debug.refresh",
        KeyButton.Get(Key.F3),
        ControllerButton.Get(GamepadButton.Start)
    );

    private static readonly Profiler.ProfilerKey BindKey = Profiler.GetProfilerKey("Poll Keybinds");

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
        using (BindKey.Push()) {
            foreach (var bind in Keybindings.Values)
                bind.Poll();
        } 
    }
}
