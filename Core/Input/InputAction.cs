using Veldrid;

namespace RenderSurface.Input;

public class InputAction {

    internal readonly InputManager Manager;

    public readonly Key Key;
    public bool IsPressed { get; internal set; } = false;
    public double Value { get; private set; } = 0;

    internal InputAction(Key key, InputManager manager) {
        Key = key;
        Manager = manager;
    }

    internal void Update() {
        Value = IsPressed ? 1 : 0;
    }
}
