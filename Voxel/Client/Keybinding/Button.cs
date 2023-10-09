using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Voxel.Client.Keybinding;

public abstract class Button {
    public static Button? FromString(string button) {
        var sub = button.Split(".");
        var first = sub[0];
        var second = sub[1];

        return first switch {
            "Key" => KeyButton.FromString(second),
            "Mouse" => MouseButton.FromString(second),
            "Controller" => ControllerButton.FromString(second),
            _ => null,
        };
    }

    public abstract bool isPressed { get; }

    public virtual float strength => isPressed ? 1 : 0;

    public abstract override string ToString();
}

public class KeyButton : Button {
    private static readonly Dictionary<Keys, KeyButton> Cache = new();

    public new static KeyButton? FromString(string value)
        => Enum.TryParse(value, out Keys key) ? null : Get(key);

    public static KeyButton Get(Keys key) {
        if (!Cache.ContainsKey(key))
            Cache[key] = new(key);
        
        return Cache[key];
    }

    public readonly Keys Key;

    public override bool isPressed => Keyboard.GetState().IsKeyDown(Key);

    private KeyButton(Keys key) {
        Key = key;
    }

    public override string ToString()
        => "Key."+Key;
}

public class MouseButton : Button {
    private static readonly Dictionary<Type, MouseButton> Cache = new();

    public new static MouseButton? FromString(string value)
        => Enum.TryParse(value, out Type type) ? Get(type) : null;

    public static MouseButton Get(Type type) {
        if (!Cache.ContainsKey(type))
            Cache[type] = new(type);
        
        return Cache[type];
    }

    public readonly Type Button;

    public override bool isPressed => GetState() == ButtonState.Pressed;

    private MouseButton(Type button) {
        Button = button;
    }

    public override string ToString()
        => "Mouse."+Button;

    private ButtonState GetState() {
        var state = Mouse.GetState();
        switch (Button) {
            case Type.Left:
                return state.LeftButton;
            case Type.Middle:
                return state.MiddleButton;
            case Type.Right:
                return state.RightButton;
            case Type.Side1:
                return state.XButton1;
            case Type.Side2:
                return state.XButton2;
        }
        return ButtonState.Released;
    }

    public enum Type {
        Left,
        Middle,
        Right,
        Side1,
        Side2
    }
}

public class ControllerButton : Button {
    private static readonly Dictionary<Buttons, ControllerButton> Cache = new();

    public new static ControllerButton? FromString(string value)
        => Enum.TryParse(value, out Buttons button) ? Get(button) : null;

    public static ControllerButton Get(Buttons button) {
        if (!Cache.ContainsKey(button))
            Cache[button] = new(button);
        
        return Cache[button];
    }

    public readonly Buttons Button;

    public override bool isPressed => GetStrength() > 0.5f;
    public override float strength => GetStrength();

    private ControllerButton(Buttons button) {
        Button = button;
    }

    public override string ToString()
        => "Controller."+Button;

    public static float Clamp(float value, float deadzone)
        => value > deadzone/100 ? value : 0;

    public float GetStrength() {
        var state = GamePad.GetState(0, GamePadDeadZone.None);
        if (!state.IsConnected)
            return 0;

        var left = state.ThumbSticks.Left;
        var right = state.ThumbSticks.Right;

        return Button switch {
            Buttons.LeftThumbstickDown => Clamp(MathF.Max(-left.Y, 0), ClientConfig.General.deadzoneLeft),
            Buttons.LeftThumbstickUp => Clamp(MathF.Max(left.Y, 0), ClientConfig.General.deadzoneLeft),
            Buttons.LeftThumbstickLeft => Clamp(MathF.Max(-left.X, 0), ClientConfig.General.deadzoneLeft),
            Buttons.LeftThumbstickRight => Clamp(MathF.Max(left.X, 0), ClientConfig.General.deadzoneLeft),
            Buttons.RightThumbstickDown => Clamp(MathF.Max(-right.Y, 0), ClientConfig.General.deadzoneRight),
            Buttons.RightThumbstickUp => Clamp(MathF.Max(right.Y, 0), ClientConfig.General.deadzoneRight),
            Buttons.RightThumbstickLeft => Clamp(MathF.Max(-right.X, 0), ClientConfig.General.deadzoneRight),
            Buttons.RightThumbstickRight => Clamp(MathF.Max(right.X, 0), ClientConfig.General.deadzoneRight),
            Buttons.LeftTrigger => state.Triggers.Left,
            Buttons.RightTrigger => state.Triggers.Right,
            _ => state.IsButtonDown(Button) ? 1 : 0
        };
    }
}
