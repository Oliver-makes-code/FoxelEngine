using Microsoft.Xna.Framework;
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
    private static Dictionary<Keys, KeyButton> cache = new();

    public new static KeyButton? FromString(string value) {
        if (!Enum.TryParse(value, out Keys key)) 
            return null;
        
        return Get(key);
    }

    public static KeyButton Get(Keys key) {
        if (!cache.ContainsKey(key))
            cache[key] = new(key);
        
        return cache[key];
    }

    public readonly Keys key;

    public override bool isPressed => Keyboard.GetState().IsKeyDown(key);

    private KeyButton(Keys key) {
        this.key = key;
    }

    public override string ToString() => "Key."+key.ToString();
}

public class MouseButton : Button {
    private static Dictionary<Type, MouseButton> cache = new();

    public new static MouseButton? FromString(string value) {
        if (!Enum.TryParse(value, out Type type)) 
            return null;
        
        return Get(type);
    }

    public static MouseButton Get(Type type) {
        if (!cache.ContainsKey(type))
            cache[type] = new(type);
        
        return cache[type];
    }

    public readonly Type button;

    public override bool isPressed => GetState() == ButtonState.Pressed;

    private MouseButton(Type button) {
        this.button = button;
    }

    public override string ToString() => "Mouse."+button.ToString();

    private ButtonState GetState() {
        var state = Mouse.GetState();
        switch (button) {
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
    private static Dictionary<Buttons, ControllerButton> cache = new();

    public new static ControllerButton? FromString(string value) {
        if (!Enum.TryParse(value, out Buttons button)) 
            return null;
        
        return Get(button);
    }

    public static ControllerButton Get(Buttons button) {
        if (!cache.ContainsKey(button))
            cache[button] = new(button);
        
        return cache[button];
    }

    public readonly Buttons button;

    public override bool isPressed => GetPressed();
    public override float strength => GetStrength();

    private ControllerButton(Buttons button) {
        this.button = button;
    }

    public override string ToString() => "Controller."+button.ToString();

    public bool GetPressed() {
        var state = GamePad.GetState(0);
        if (!state.IsConnected)
            return false;
        return state.IsButtonDown(button);
    }

    public static float Clamp(float value, float deadzone) => value > deadzone/100 ? value : 0;

    public float GetStrength() {
        var state = GamePad.GetState(0, GamePadDeadZone.None);
        if (!state.IsConnected)
            return 0;

        var left = state.ThumbSticks.Left;
        var right = state.ThumbSticks.Right;

        switch (button) {
            case Buttons.LeftThumbstickDown:
                return Clamp(MathF.Max(-left.Y, 0), ClientConfig.General.DeadzoneLeft);
            case Buttons.LeftThumbstickUp:
                return Clamp(MathF.Max(left.Y, 0), ClientConfig.General.DeadzoneLeft);
            case Buttons.LeftThumbstickLeft:
                return Clamp(MathF.Max(-left.X, 0), ClientConfig.General.DeadzoneLeft);
            case Buttons.LeftThumbstickRight:
                return Clamp(MathF.Max(left.X, 0), ClientConfig.General.DeadzoneLeft);

            case Buttons.RightThumbstickDown:
                return Clamp(MathF.Max(-right.Y, 0), ClientConfig.General.DeadzoneRight);
            case Buttons.RightThumbstickUp:
                return Clamp(MathF.Max(right.Y, 0), ClientConfig.General.DeadzoneRight);
            case Buttons.RightThumbstickLeft:
                return Clamp(MathF.Max(-right.X, 0), ClientConfig.General.DeadzoneRight);
            case Buttons.RightThumbstickRight:
                return Clamp(MathF.Max(right.X, 0), ClientConfig.General.DeadzoneRight);
            
            case Buttons.LeftTrigger:
                return state.Triggers.Left;
            case Buttons.RightTrigger:
                return state.Triggers.Right;
        }
        return base.strength;
    }
}
