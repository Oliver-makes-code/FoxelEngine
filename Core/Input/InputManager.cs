using GlmSharp;
using Veldrid;
using Veldrid.Sdl2;

namespace RenderSurface.Input;

public class InputManager {
    public readonly Game Game;

    public vec2 MouseDelta => new(Game.nativeWindow.MouseDelta.X, Game.nativeWindow.MouseDelta.Y);

    
    private Dictionary<Key, InputAction> actions = new();

    public InputManager(Game game) {
        Game = game;

        game.nativeWindow.KeyDown += NativeWindowOnKeyDown;
        game.nativeWindow.KeyUp += NativeWindowOnKeyUp;

        game.nativeWindow.MouseDown += NativeWindowOnMouseDown;
        game.nativeWindow.MouseUp += NativeWindowOnMouseUp;
        game.nativeWindow.MouseMove += NativeWindowOnMouseMove;
        game.nativeWindow.MouseWheel += NativeWindowOnMouseWheel;
    }

    public InputAction Register(Key key) {
        if (!actions.TryGetValue(key, out var value)) {
            value = new(key, this);
            value.Update();

            actions[key] = value;
        }

        return value;
    }

    private void NativeWindowOnKeyDown(KeyEvent obj) {
        if (!actions.TryGetValue(obj.Key, out var value))
            return;

        value.isPressed = true;
    }

    private void NativeWindowOnKeyUp(KeyEvent obj) {
        if (!actions.TryGetValue(obj.Key, out var value))
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
}
