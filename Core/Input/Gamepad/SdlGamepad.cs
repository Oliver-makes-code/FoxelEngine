using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid.Sdl2;

namespace RenderSurface.Input.Gamepad; 

public sealed class SdlGamepad {
    public readonly int Index;
    public readonly string ControllerName;
    public readonly SDL_GameController Controller;

    private readonly Dictionary<GamepadAxis, double> Axes = new();
    private readonly Dictionary<GamepadButton, bool> Buttons = new();

    public double this[GamepadAxis axis] {
        get {
            switch (axis) {
                case GamepadAxis.LeftXNegative:
                    return Math.Max(-this[GamepadAxis.LeftX], 0);
                
                case GamepadAxis.LeftXPositive:
                    return Math.Max(this[GamepadAxis.LeftX], 0);
                
                case GamepadAxis.LeftYNegative:
                    return Math.Max(-this[GamepadAxis.LeftY], 0);
                
                case GamepadAxis.LeftYPositive:
                    return Math.Max(this[GamepadAxis.LeftY], 0);
                
                case GamepadAxis.RightXNegative:
                    return Math.Max(-this[GamepadAxis.RightX], 0);
                
                case GamepadAxis.RightXPositive:
                    return Math.Max(this[GamepadAxis.RightX], 0);
                
                case GamepadAxis.RightYNegative:
                    return Math.Max(-this[GamepadAxis.RightY], 0);
                
                case GamepadAxis.RightYPositive:
                    return Math.Max(this[GamepadAxis.RightY], 0);
                
                default:
                    Axes.TryGetValue(axis, out double value);
                    
                    if (axis != GamepadAxis.LeftTrigger && axis != GamepadAxis.RightTrigger)
                        return value;
                    
                    value += 1;
                    value /= 2;
                    return value;
            }
        }
        private set => Axes[axis] = value;
    }

    public bool this[GamepadButton button] {
        get => Buttons.TryGetValue(button, out bool value) && value;
        private set => Buttons[button] = value;
    }

    public SdlGamepad(int index) {
        Controller = Sdl2Native.SDL_GameControllerOpen(index);
        var joystick = Sdl2Native.SDL_GameControllerGetJoystick(Controller);
        Index = Sdl2Native.SDL_JoystickInstanceID(joystick);
        
        // SAFETY: We know Sdl2Native.SDL_GameControllerName returns a valid pointer to a C String (or nullptr).
        unsafe {
            void *ptr = Sdl2Native.SDL_GameControllerName(Controller);
            ControllerName = Marshal.PtrToStringUTF8((nint)ptr) ?? "Controller";
        }
    }
    
    public void Disconnect() {
        Sdl2Native.SDL_GameControllerClose(Controller);
    }

    public override bool Equals(object? obj) {
        if (obj is not SdlGamepad other)
            return false;
        return other.Index == Index;
    }

    public override int GetHashCode()
        => Index;

    public override string ToString()
        => $"[{Index}] {ControllerName}";

    public static bool operator == (SdlGamepad? rhs, object? o)
        => rhs?.Equals(o) ?? false;

    public static bool operator != (SdlGamepad? rhs, object? o)
        => !(rhs == o);

    public void OnAxisMotion(GamepadAxis axis, short rawValue) {
        this[axis] = ToDouble(rawValue);
    }

    public void OnButtonPress(GamepadButton button, bool pressed) {
        this[button] = pressed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double ToDouble(short value)
        => value < 0 ? -(value / (double)short.MinValue) : value / (double)short.MaxValue;
}
