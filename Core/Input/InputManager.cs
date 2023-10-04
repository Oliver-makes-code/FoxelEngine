using GlmSharp;
using Veldrid;
using Veldrid.Sdl2;

namespace RenderSurface.Input;

public class InputManager {
    public Game Game { get; private set; }

    public vec2 MouseDelta => new vec2(Game.NativeWindow.MouseDelta.X, Game.NativeWindow.MouseDelta.Y);

    private Dictionary<Key, InputAction> _actions = new();

    public InputManager(Game game) {
        Game = game;

        game.NativeWindow.KeyDown += NativeWindowOnKeyDown;
        game.NativeWindow.KeyUp += NativeWindowOnKeyUp;

        game.NativeWindow.MouseDown += NativeWindowOnMouseDown;
        game.NativeWindow.MouseUp += NativeWindowOnMouseUp;
        game.NativeWindow.MouseMove += NativeWindowOnMouseMove;
        game.NativeWindow.MouseWheel += NativeWindowOnMouseWheel;
    }

    public InputAction Register(Key key) {
        if (!_actions.TryGetValue(key, out var value)) {
            value = new InputAction(key, this);
            value.Update();

            _actions[key] = value;
        }

        return value;
    }

    private void NativeWindowOnKeyDown(KeyEvent obj) {
        if (!_actions.TryGetValue(obj.Key, out var value))
            return;

        value.IsPressed = true;
    }

    private void NativeWindowOnKeyUp(KeyEvent obj) {
        if (!_actions.TryGetValue(obj.Key, out var value))
            return;

        value.IsPressed = false;
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
