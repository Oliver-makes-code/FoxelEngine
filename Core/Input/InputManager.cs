using System.Runtime.CompilerServices;
using GlmSharp;
using RenderSurface.Input.Gamepad;
using Veldrid;
using Veldrid.Sdl2;

namespace RenderSurface.Input;

public sealed class InputManager : IDisposable {
    public readonly Game Game;

    public vec2 MouseDelta => new(Game.NativeWindow.MouseDelta.X, Game.NativeWindow.MouseDelta.Y);

    private readonly HashSet<Key> PressedKeys = new();
    private readonly Dictionary<Key, InputAction> Actions = new();
    private readonly HashSet<SdlGamepad> Gamepads = new();

    public InputManager(Game game) {
        Game = game;

        game.NativeWindow.KeyDown += NativeWindowOnKeyDown;
        game.NativeWindow.KeyUp += NativeWindowOnKeyUp;

        game.NativeWindow.MouseDown += NativeWindowOnMouseDown;
        game.NativeWindow.MouseUp += NativeWindowOnMouseUp;
        game.NativeWindow.MouseMove += NativeWindowOnMouseMove;
        game.NativeWindow.MouseWheel += NativeWindowOnMouseWheel;
        
        Sdl2Events.Subscribe(OnSdlEvent);
    }

    public bool IsKeyPressed(Key key)
        => PressedKeys.Contains(key);

    public InputAction Register(Key key) {
        if (Actions.TryGetValue(key, out var value))
            return value;
        
        value = new(key, this);
        value.Update();

        Actions[key] = value;

        return value;
    }
    
    public void Dispose() {
        Sdl2Events.Unsubscribe(OnSdlEvent);
    }

    private void NativeWindowOnKeyDown(KeyEvent obj) {
        PressedKeys.Add(obj.Key);

        if (!Actions.TryGetValue(obj.Key, out var value))
            return;

        value.isPressed = true;
    }

    private void NativeWindowOnKeyUp(KeyEvent obj) {
        PressedKeys.Remove(obj.Key);

        if (!Actions.TryGetValue(obj.Key, out var value))
            return;

        value.isPressed = false;
    }

    private void NativeWindowOnMouseDown(MouseEvent mouseEvent) {

    }

    private void NativeWindowOnMouseUp(MouseEvent mouseEvent) {

    }

    private void NativeWindowOnMouseMove(MouseMoveEventArgs mouseMoveEventArgs) {

    }

    private void NativeWindowOnMouseWheel(MouseWheelEventArgs mouseWheelEventArgs) {

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
                    ev.type == SDL_EventType.ControllerButtonUp
                );
                break;
        }
    }
}
