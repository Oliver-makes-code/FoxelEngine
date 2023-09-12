using Silk.NET.Input;

namespace Voxel.Client;

public static class Keybinds {
    public static readonly List<Keybind> Registered = new();

    public static void SendChange(Key key, bool pressed) {
        Registered.ForEach(bind => {
            if (bind.BoundKey != key)
                return;
            if (pressed)
                bind.Press();
            else bind.Release();
        });
    }

    public static void UpdateAll() {
        Registered.ForEach(bind => bind.Update());
    }
}

public class Keybind {
    public readonly Key DefaultKey;
    public readonly string Name;
    public Key BoundKey;

    public bool Pressed {get; private set;} = false;
    public bool JustPressed {get; private set;} = false;

    public static Keybind Create(string name, Key defaultKey) {
        Keybind keybind = new(name, defaultKey);

        Keybinds.Registered.Add(keybind);

        return keybind;
    }

    private Keybind(string name, Key defaultKey) {
        Name = name;
        DefaultKey = defaultKey;
    }

    public void Press() {
        JustPressed = true;
        Pressed = true;
    }

    public void Release() {
        JustPressed = false;
        Pressed = false;
    }

    public void Update() {
        JustPressed = false;
    }
}
