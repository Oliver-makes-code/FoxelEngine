using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid.Sdl2;

namespace RenderSurface.Input.Gamepad; 

public sealed class SdlGamepad : IDisposable {
    public readonly int Index;
    public readonly string ControllerName;
    public readonly SDL_GameController Controller;

    private readonly Dictionary<GamepadAxis, double> Axes = new();
    private readonly Dictionary<GamepadButton, bool> Buttons = new();

    public double this[GamepadAxis axis] {
        get {
            Axes.TryGetValue(axis, out double value);
            return value;
        }
        private set => Axes[axis] = value;
    }

    public double this[SDL_GameControllerAxis axis] {
        get => this[axis.IntoWrapped()];
        private set => this[axis.IntoWrapped()] = value;
    }

    public bool this[GamepadButton button] {
        get => Buttons.TryGetValue(button, out bool value) && value;
        private set => Buttons[button] = value;
    }

    public bool this[SDL_GameControllerButton button] {
        get => this[button.IntoWrapped()];
        private set => this[button.IntoWrapped()] = value;
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
        
        Sdl2Events.Subscribe(OnSdlEvent);
    }
    
    public void Dispose() {
        Sdl2Events.Unsubscribe(OnSdlEvent);
        Sdl2Native.SDL_GameControllerClose(Controller);
    }

    private void OnAxisMotion(ref SDL_ControllerAxisEvent ev) {
        if (ev.which != Index)
            return;
        
        this[ev.axis] = ToDouble(ev.value);
    }

    private void OnButtonPress(ref SDL_ControllerButtonEvent ev, bool pressed) {
        if (ev.which != Index)
            return;

        this[ev.button] = pressed;
    }

    private void OnSdlEvent(ref SDL_Event ev) {
        switch (ev.type) {
            case SDL_EventType.ControllerAxisMotion:
                OnAxisMotion(
                    ref Unsafe.As<SDL_Event, SDL_ControllerAxisEvent>(ref ev)
                );
                break;
            case SDL_EventType.ControllerButtonUp:
            case SDL_EventType.ControllerButtonDown:
                OnButtonPress(
                    ref Unsafe.As<SDL_Event, SDL_ControllerButtonEvent>(ref ev),
                    ev.type == SDL_EventType.ControllerButtonUp
                );
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double ToDouble(short value)
        => value < 0 ? -(value / (double)short.MinValue) : value / (double)short.MaxValue;
}
