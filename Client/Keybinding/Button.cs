using System;
using System.Collections.Generic;
using GlmSharp;
using RenderSurface.Input.Gamepad;
using Veldrid;
using VMouseButton = Veldrid.MouseButton;

namespace Voxel.Client.Keybinding;

public abstract class Button {
    public static Button? FromString(string button) {
        string[] sub = button.Split(".");
        string first = sub[0];
        string second = sub[1];

        return first switch {
            "Key" => KeyButton.FromString(second),
            "Mouse" => MouseButton.FromString(second),
            "Button" => ControllerButton.FromString(second),
            "Axis" => ControllerAxisButton.FromString(second),
            _ => null,
        };
    }

    public abstract bool isPressed { get; }

    public virtual double strength => isPressed ? 1 : 0;

    public abstract override string ToString();
}

public class KeyButton : Button {
    private static readonly Dictionary<Key, KeyButton> Cache = new();

    public new static KeyButton? FromString(string value)
        => Enum.TryParse(value, out Key key) ? Get(key) : null;

    public static KeyButton Get(Key key) {
        if (!Cache.ContainsKey(key))
            Cache[key] = new(key);
        
        return Cache[key];
    }

    public readonly Key Key;

    public override bool isPressed => VoxelClient.Instance.InputManager.IsKeyPressed(Key);

    private KeyButton(Key key) {
        Key = key;
    }

    public override string ToString()
        => $"Key.{Key}";
}

public class MouseButton : Button {
    private static readonly Dictionary<VMouseButton, MouseButton> Cache = new();

    public new static MouseButton? FromString(string value)
        => Enum.TryParse(value, out VMouseButton type) ? Get(type) : null;

    public static MouseButton Get(VMouseButton type) {
        if (!Cache.ContainsKey(type))
            Cache[type] = new(type);
        
        return Cache[type];
    }

    public readonly VMouseButton Button;

    public override bool isPressed => VoxelClient.Instance.InputManager.IsMouseButtonPressed(Button);

    private MouseButton(VMouseButton button) {
        Button = button;
    }

    public override string ToString()
        => "Mouse."+Button;
}

public class ControllerButton : Button {
    private static readonly Dictionary<GamepadButton, ControllerButton> Cache = new();

    public new static ControllerButton? FromString(string value)
        => Enum.TryParse(value, out GamepadButton button) ? Get(button) : null;

    public static ControllerButton Get(GamepadButton button) {
        if (!Cache.ContainsKey(button))
            Cache[button] = new(button);

        return Cache[button];
    }

    public readonly GamepadButton Button;

    public override bool isPressed => VoxelClient.Instance.InputManager.IsButtonPressed(Button);

    public ControllerButton(GamepadButton button) {
        Button = button;
    }
    
    public override string ToString()
        => $"Button.{Button}";
}

public class ControllerAxisButton : Button {
    private static readonly Dictionary<GamepadAxis, ControllerAxisButton> Cache = new();
    
    public new static ControllerAxisButton? FromString(string value)
        => Enum.TryParse(value, out GamepadAxis axis) ? Get(axis) : null;
    
    public static ControllerAxisButton Get(GamepadAxis axis) {
        if (!Cache.ContainsKey(axis))
            Cache[axis] = new(axis);

        return Cache[axis];
    }

    public readonly GamepadAxis Axis;

    public override double strength => GetAxisStrength(Axis);

    public override bool isPressed => strength > 0.25;

    public ControllerAxisButton(GamepadAxis axis) {
        Axis = axis;
    }

    public override string ToString()
        => $"Axis.{Axis}";

    private static double GetAxisStrength(GamepadAxis axis) {
        var inputManager = VoxelClient.Instance.InputManager;
        if (axis == GamepadAxis.RightX || axis == GamepadAxis.RightY) {
            int i = (int)axis - 2;
            var vec = new dvec2(
                inputManager.GetAxisStrength(GamepadAxis.RightX),
                inputManager.GetAxisStrength(GamepadAxis.RightY)
            );
            if (vec.Length < ClientConfig.General.deadzoneRight)
                return 0;

            if (vec[i] < ClientConfig.General.deadzoneRight * 0.5)
                return 0;

            return vec[i];
        }
        if (axis == GamepadAxis.LeftX || axis == GamepadAxis.LeftY) {
            int i = (int)axis;
            var vec = new dvec2(
                inputManager.GetAxisStrength(GamepadAxis.LeftX),
                inputManager.GetAxisStrength(GamepadAxis.LeftY)
            );
            if (vec.Length < ClientConfig.General.deadzoneLeft)
                return 0;

            if (vec[i] < ClientConfig.General.deadzoneLeft * 0.5)
                return 0;

            return vec[i];
        }

        return inputManager.GetAxisStrength(axis);
    }
}
