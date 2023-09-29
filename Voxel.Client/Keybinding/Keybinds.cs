using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Voxel.Client.Keybinding;

public static class Keybinds {
    public static Dictionary<string, Keybind> binds = new();

    public static readonly Keybind pause = new(
        "pause",
        KeyButton.Get(Keys.Escape),
        ControllerButton.Get(Buttons.Start)
    );

    public static readonly Keybind forward = new(
        "movement.forward",
        KeyButton.Get(Keys.W),
        ControllerButton.Get(Buttons.LeftThumbstickUp)
    );

    public static readonly Keybind backward = new(
        "movement.backward",
        KeyButton.Get(Keys.S),
        ControllerButton.Get(Buttons.LeftThumbstickDown)
    );

    public static readonly Keybind strafeLeft = new(
        "movement.strafe.left",
        KeyButton.Get(Keys.A),
        ControllerButton.Get(Buttons.LeftThumbstickLeft)
    );

    public static readonly Keybind strafeRight = new(
        "movement.strafe.right",
        KeyButton.Get(Keys.D),
        ControllerButton.Get(Buttons.LeftThumbstickRight)
    );

    public static readonly Keybind jump = new(
        "movement.jump",
        KeyButton.Get(Keys.Space),
        ControllerButton.Get(Buttons.A)
    );

    public static readonly Keybind crouch = new(
        "movement.crouch",
        KeyButton.Get(Keys.LeftShift),
        ControllerButton.Get(Buttons.RightStick)
    );

    public static readonly Keybind lookUp = new(
        "camera.up",
        KeyButton.Get(Keys.Up),
        ControllerButton.Get(Buttons.RightThumbstickUp)
    );

    public static readonly Keybind lookDown = new(
        "camera.down",
        KeyButton.Get(Keys.Down),
        ControllerButton.Get(Buttons.RightThumbstickDown)
    );

    public static readonly Keybind lookLeft = new(
        "camera.left",
        KeyButton.Get(Keys.Left),
        ControllerButton.Get(Buttons.RightThumbstickLeft)
    );

    public static readonly Keybind lookRight = new(
        "camera.right",
        KeyButton.Get(Keys.Right),
        ControllerButton.Get(Buttons.RightThumbstickRight)
    );

    public static readonly Keybind use = new(
        "action.use",
        MouseButton.Get(MouseButton.Type.Right),
        ControllerButton.Get(Buttons.RightTrigger)
    );
    
    public static readonly Keybind attack = new(
        "action.attack",
        MouseButton.Get(MouseButton.Type.Left),
        ControllerButton.Get(Buttons.LeftTrigger)
    );

    public static void ReadFromConfig() {
        foreach (var bind in ClientConfig.Keybindings) {
            if (!binds.ContainsKey(bind.Key))
                continue;
            var bindToSet = binds[bind.Key];
            bindToSet.ReadButtonString(bind.Value);
        }
    }

    public static void WriteToConfig() {
        Dictionary<string, string[]> keybinds = new();
        
        foreach (var bind in binds) {
            keybinds[bind.Key] = bind.Value.GetButtonString();
        }

        ClientConfig.Keybindings = keybinds;
    }

    public static void Poll() {
        foreach (var bind in binds.Values)
            bind.Poll();
    }
}
