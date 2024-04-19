// TODO: Separate this out into different files ffs
// I have no clue why we haven't done this yet lmao
// But right now it's more work than it's worth

using System;
using System.Collections.Generic;
using GlmSharp;
using Veldrid;
using Voxel.Core.Input.Gamepad;
using VMouseButton = Veldrid.MouseButton;

namespace Voxel.Client.Keybinding;

public abstract class Button {
    public abstract bool isPressed { get; }

    public virtual double strength => isPressed ? 1 : 0;

    public virtual dvec2 axis => new(strength, 0);

    public static Button? FromString(string button) {
        string[] sub = button.Split(".");
        string first = sub[0];
        string second = sub[1];

        return first switch {
            "Key" => KeyButton.FromString(second),
            "Mouse" => MouseButton.FromString(second),
            "Button" => ControllerButton.FromString(second),
            "Trigger" => ControllerTriggerButton.FromString(second),
            "Joystick" => ControllerJoystickButton.FromString(second),
            _ => null,
        };
    }

    public abstract override string ToString();
}

public class KeyButton : Button {
    private static readonly Dictionary<Key, KeyButton> Cache = [];

    public readonly Key Key;

    public override bool isPressed => VoxelClient.instance?.inputManager?.IsKeyPressed(Key) ?? false;

    private KeyButton(Key key) {
        Key = key;
    }

    public new static KeyButton? FromString(string value)
        => Enum.TryParse(value, out Key key) ? Get(key) : null;

    public static KeyButton Get(Key key) {
        if (!Cache.ContainsKey(key))
            Cache[key] = new(key);
        
        return Cache[key];
    }

    public override string ToString()
        => $"Key.{Key}";
}

public class MouseButton : Button {
    private static readonly Dictionary<VMouseButton, MouseButton> Cache = [];

    public readonly VMouseButton Button;

    public override bool isPressed => VoxelClient.instance?.inputManager?.IsMouseButtonPressed(Button) ?? false;

    private MouseButton(VMouseButton button) {
        Button = button;
    }

    public new static MouseButton? FromString(string value)
        => Enum.TryParse(value, out VMouseButton type) ? Get(type) : null;

    public static MouseButton Get(VMouseButton type) {
        if (!Cache.ContainsKey(type))
            Cache[type] = new(type);
        
        return Cache[type];
    }

    public override string ToString()
        => "Mouse."+Button;
}

public class ControllerButton : Button {
    private static readonly Dictionary<GamepadButton, ControllerButton> Cache = [];


    public readonly GamepadButton Button;

    public override bool isPressed => VoxelClient.instance?.inputManager?.IsButtonPressed(Button) ?? false;

    private ControllerButton(GamepadButton button) {
        Button = button;
    }

    public new static ControllerButton? FromString(string value)
        => Enum.TryParse(value, out GamepadButton button) ? Get(button) : null;

    public static ControllerButton Get(GamepadButton button) {
        if (!Cache.ContainsKey(button))
            Cache[button] = new(button);

        return Cache[button];
    }

    
    public override string ToString()
        => $"Button.{Button}";
}

public class ControllerTriggerButton : Button {
    private static readonly Dictionary<GamepadTrigger, ControllerTriggerButton> Cache = [];


    public readonly GamepadTrigger Trigger;

    public override double strength => VoxelClient.instance?.inputManager?.GetAxisStrength(Trigger.GetAxis()) ?? 0;
    
    public override bool isPressed => strength > 0.25;

    private ControllerTriggerButton(GamepadTrigger trigger) {
        Trigger = trigger;
    }

    public new static ControllerTriggerButton? FromString(string value)
        => Enum.TryParse(value, out GamepadTrigger trigger) ? Get(trigger) : null;
    
    public static ControllerTriggerButton Get(GamepadTrigger gamepadTrigger) {
        if (!Cache.ContainsKey(gamepadTrigger))
            Cache[gamepadTrigger] = new(gamepadTrigger);

        return Cache[gamepadTrigger];
    }
    
    public override string ToString()
        => $"Trigger.{Trigger}";

    public enum GamepadTrigger {
        Left,
        Right
    }
}

public class ControllerJoystickButton : Button {
    private static readonly Dictionary<GamepadJoystick, ControllerJoystickButton> Cache = [];


    public readonly GamepadJoystick Joystick;

    public override bool isPressed => GetAxisStrength().LengthSqr > 0;

    public override double strength => GetAxisStrength().Length;

    public override dvec2 axis => GetAxisStrength();

    private ControllerJoystickButton(GamepadJoystick joystick) {
        Joystick = joystick;
    }
    
    public new static ControllerJoystickButton? FromString(string value)
        => Enum.TryParse(value, out GamepadJoystick axis) ? Get(axis) : null;
    
    public static ControllerJoystickButton Get(GamepadJoystick axis) {
        if (!Cache.ContainsKey(axis))
            Cache[axis] = new(axis);

        return Cache[axis];
    }

    
    public override string ToString()
        => $"Joystick.{Joystick}";

    private dvec2 GetAxisStrength() {
        double deadzone;
        double snap;
        if (Joystick == GamepadJoystick.Left) {
            deadzone = ClientConfig.General.deadzoneLeft;
            snap = ClientConfig.General.snapLeft;
        } else {
            deadzone = ClientConfig.General.deadzoneRight;
            snap = ClientConfig.General.snapRight;
        }

        Func<GamepadAxis, double> GetAxisStrength = VoxelClient.instance != null ? VoxelClient.instance.inputManager.GetAxisStrength : _ => 0;

        var vec = new dvec2(
            GetAxisStrength(Joystick.GetAxisX()),
            GetAxisStrength(Joystick.GetAxisY())
        );

        for (int i = 0; i < 2; i++)
            if (Math.Abs(vec[i]) < snap)
                vec[i] = 0;

        if (deadzone <= 0)
            return vec;
        if (deadzone > 1)
            return new(0);

        if (vec.LengthSqr < deadzone * deadzone)
            return new(0);

        vec -= (dvec2) dvec2.Sign(vec) * deadzone;
        
        vec *= 1 / (1 - deadzone);
        
        return vec;
    }

    public enum GamepadJoystick {
        Left,
        Right
    }
}

public static class AxisExtensions {
    public static GamepadAxis GetAxis(this ControllerTriggerButton.GamepadTrigger gamepadTrigger)
        => gamepadTrigger switch {
            ControllerTriggerButton.GamepadTrigger.Left => GamepadAxis.LeftTrigger,
            ControllerTriggerButton.GamepadTrigger.Right => GamepadAxis.RightTrigger,
            _ => GamepadAxis.Invalid
        };
    public static GamepadAxis GetAxisX(this ControllerJoystickButton.GamepadJoystick gamepadTrigger)
        => gamepadTrigger switch {
            ControllerJoystickButton.GamepadJoystick.Left => GamepadAxis.LeftX,
            ControllerJoystickButton.GamepadJoystick.Right => GamepadAxis.RightX,
            _ => GamepadAxis.Invalid
        };
    public static GamepadAxis GetAxisY(this ControllerJoystickButton.GamepadJoystick gamepadTrigger)
        => gamepadTrigger switch {
            ControllerJoystickButton.GamepadJoystick.Left => GamepadAxis.LeftY,
            ControllerJoystickButton.GamepadJoystick.Right => GamepadAxis.RightY,
            _ => GamepadAxis.Invalid
        };
}
