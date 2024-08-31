using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid.Sdl2;

namespace Foxel.Core.Input.Gamepad; 

public sealed class SdlGamepad {
    public readonly int Index;
    public readonly string ControllerName;
    public readonly SDL_GameController Controller;

    private readonly Dictionary<GamepadAxis, float> Axes = new();
    private readonly Dictionary<GamepadButton, bool> Buttons = new();

    public float this[GamepadAxis axis] {
        get {
            Axes.TryGetValue(axis, out float value);
            
            return value;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float ToFloat(short value)
        => value < 0 ? -(value / (float)short.MinValue) : value / (float)short.MaxValue;
    
    public void Disconnect()
        => Sdl2Native.SDL_GameControllerClose(Controller);

    public override bool Equals(object? obj)
        => obj is not SdlGamepad other
        ? false
        : other.Index == Index;

    public override int GetHashCode()
        => Index;

    public override string ToString()
        => $"[{Index}] {ControllerName}";

    public void OnAxisMotion(GamepadAxis axis, short rawValue)
        => this[axis] = ToFloat(rawValue);

    public void OnButtonPress(GamepadButton button, bool pressed) 
        => this[button] = pressed;

    public static bool operator == (SdlGamepad? rhs, object? o)
        => rhs?.Equals(o) ?? false;

    public static bool operator != (SdlGamepad? rhs, object? o)
        => !(rhs == o);
}
