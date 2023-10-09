using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Voxel.Client.Keybinding;

public static class Keybinds {
    public static readonly Dictionary<string, Keybind> Keybindings = new();

    public static readonly Keybind Pause = new(
        "pause",
        KeyButton.Get(Keys.Escape),
        ControllerButton.Get(Buttons.Start)
    );

    public static readonly Keybind Forward = new(
        "movement.forward",
        KeyButton.Get(Keys.W),
        ControllerButton.Get(Buttons.LeftThumbstickUp)
    );

    public static readonly Keybind Backward = new(
        "movement.backward",
        KeyButton.Get(Keys.S),
        ControllerButton.Get(Buttons.LeftThumbstickDown)
    );

    public static readonly Keybind StrafeLeft = new(
        "movement.strafe.left",
        KeyButton.Get(Keys.A),
        ControllerButton.Get(Buttons.LeftThumbstickLeft)
    );

    public static readonly Keybind StrafeRight = new(
        "movement.strafe.right",
        KeyButton.Get(Keys.D),
        ControllerButton.Get(Buttons.LeftThumbstickRight)
    );

    public static readonly Keybind Jump = new(
        "movement.jump",
        KeyButton.Get(Keys.Space),
        ControllerButton.Get(Buttons.A)
    );

    public static readonly Keybind Crouch = new(
        "movement.crouch",
        KeyButton.Get(Keys.LeftShift),
        ControllerButton.Get(Buttons.RightStick)
    );

    public static readonly Keybind LookUp = new(
        "camera.up",
        KeyButton.Get(Keys.Up),
        ControllerButton.Get(Buttons.RightThumbstickUp)
    );

    public static readonly Keybind LookDown = new(
        "camera.down",
        KeyButton.Get(Keys.Down),
        ControllerButton.Get(Buttons.RightThumbstickDown)
    );

    public static readonly Keybind LookLeft = new(
        "camera.left",
        KeyButton.Get(Keys.Left),
        ControllerButton.Get(Buttons.RightThumbstickLeft)
    );

    public static readonly Keybind LookRight = new(
        "camera.right",
        KeyButton.Get(Keys.Right),
        ControllerButton.Get(Buttons.RightThumbstickRight)
    );

    public static readonly Keybind Use = new(
        "action.use",
        MouseButton.Get(MouseButton.Type.Right),
        ControllerButton.Get(Buttons.LeftTrigger)
    );
    
    public static readonly Keybind Attack = new(
        "action.attack",
        MouseButton.Get(MouseButton.Type.Left),
        ControllerButton.Get(Buttons.RightTrigger)
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
