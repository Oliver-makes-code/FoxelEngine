using Veldrid;

namespace RenderSurface.Input;

public class InputAction {
    internal readonly InputManager Manager;

    public readonly Key Key;
    public bool isPressed { get; internal set; }
    public double value { get; private set; }

    internal InputAction(Key key, InputManager manager) {
        Key = key;
        Manager = manager;
    }

    internal void Update() {
        value = isPressed ? 1 : 0;
    }
}
