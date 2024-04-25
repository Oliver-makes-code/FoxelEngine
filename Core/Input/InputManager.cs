using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using GlmSharp;
using Veldrid;
using Veldrid.Sdl2;
using Voxel.Core.Input.Gamepad;

namespace Voxel.Core.Input;

public sealed class InputManager : IDisposable {
    public readonly Game Game;

    public vec2 MouseDelta => new(Game.nativeWindow!.MouseDelta.X, Game.nativeWindow.MouseDelta.Y);

    public float mouseWheelDelta { get; set; }

    private readonly HashSet<Key> PressedKeys = [];
    private readonly List<SdlGamepad> Gamepads = [];
    private readonly HashSet<MouseButton> PressedMouseButtons = [];

    public InputManager(Game game) {
        Game = game;

        game.nativeWindow!.KeyDown += NativeWindowOnKeyDown;
        game.nativeWindow.KeyUp += NativeWindowOnKeyUp;

        game.nativeWindow.MouseDown += NativeWindowOnMouseDown;
        game.nativeWindow.MouseUp += NativeWindowOnMouseUp;
        game.nativeWindow.MouseMove += NativeWindowOnMouseMove;
        game.nativeWindow.MouseWheel += NativeWindowOnMouseWheel;
        
        Sdl2Events.Subscribe(OnSdlEvent);
    }

    public bool IsKeyPressed(Key key)
        => PressedKeys.Contains(key);

    public bool IsMouseButtonPressed(MouseButton button)
        => PressedMouseButtons.Contains(button);

    public bool IsButtonPressed(GamepadButton button, int index) {
        if (index >= Gamepads.Count)
            return false;
        return Gamepads[index][button];
    }

    public float GetAxisStrength(GamepadAxis axis, int index) {
        if (index >= Gamepads.Count)
            return 0;
        return Gamepads[index][axis];
    }

    public ReadOnlyCollection<SdlGamepad> GetRawGamepads()
        => new(Gamepads.ToList());
    
    public void Dispose() {
        Sdl2Events.Unsubscribe(OnSdlEvent);
    }

    private void NativeWindowOnKeyDown(KeyEvent obj) {
        PressedKeys.Add(obj.Key);
    }

    private void NativeWindowOnKeyUp(KeyEvent obj) {
        PressedKeys.Remove(obj.Key);
    }

    private void NativeWindowOnMouseDown(MouseEvent mouseEvent) {
        PressedMouseButtons.Add(mouseEvent.MouseButton);
    }

    private void NativeWindowOnMouseUp(MouseEvent mouseEvent) {
        PressedMouseButtons.Remove(mouseEvent.MouseButton);
    }

    private void NativeWindowOnMouseMove(MouseMoveEventArgs mouseMoveEventArgs) {

    }

    private void NativeWindowOnMouseWheel(MouseWheelEventArgs mouseWheelEventArgs) {
        mouseWheelDelta = mouseWheelEventArgs.WheelDelta;
    }

    private void OnGamepadAdd(ref SDL_ControllerDeviceEvent ev) {
        Gamepads.Add(new(ev.which));
    }

    private void OnGamepadRemove(ref SDL_ControllerDeviceEvent ev) {
        int which = ev.which;
        
        var gamepad = Gamepads.FirstOrDefault(it => it.Index == which);
        if (gamepad == null)
            return;
        
        gamepad.Disconnect();
        Gamepads.Remove(gamepad);
    }

    private void OnAxisMotion(ref SDL_ControllerAxisEvent ev) {
        int which = ev.which;
        Gamepads.FirstOrDefault(it => it.Index == which)?.OnAxisMotion(ev.axis.IntoWrapped(), ev.value);
    }

    private void OnButtonPress(ref SDL_ControllerButtonEvent ev, bool pressed) {
        int which = ev.which;
        Gamepads.FirstOrDefault(it => it.Index == which)?.OnButtonPress(ev.button.IntoWrapped(), pressed);
    }

    private void OnSdlEvent(ref SDL_Event ev) {
        switch (ev.type) {
            case SDL_EventType.ControllerDeviceAdded:
                OnGamepadAdd(ref Unsafe.As<SDL_Event, SDL_ControllerDeviceEvent>(ref ev));
                break;
            case SDL_EventType.ControllerDeviceRemoved:
                OnGamepadRemove(ref Unsafe.As<SDL_Event, SDL_ControllerDeviceEvent>(ref ev));
                break;
            case SDL_EventType.ControllerAxisMotion:
                OnAxisMotion(
                    ref Unsafe.As<SDL_Event, SDL_ControllerAxisEvent>(ref ev)
                );
                break;
            case SDL_EventType.ControllerButtonUp:
            case SDL_EventType.ControllerButtonDown:
                OnButtonPress(
                    ref Unsafe.As<SDL_Event, SDL_ControllerButtonEvent>(ref ev),
                    ev.type == SDL_EventType.ControllerButtonDown
                );
                break;
        }
    }
}
