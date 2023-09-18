using Microsoft.Xna.Framework.Input;

namespace Voxel.Client.Keybinding;

public static class Keybinds {
    public static Dictionary<string, Keybind> binds = new();

    public static readonly Keybind forward = new(
        "movement.forward",
        KeyButton.Get(Keys.W)
    );
    public static readonly Keybind backward = new(
        "movement.backward",
        KeyButton.Get(Keys.S)
    );
    public static readonly Keybind strafeLeft = new(
        "movement.strafe.left",
        KeyButton.Get(Keys.A)
    );
    public static readonly Keybind strafeRight = new(
        "movement.strafe.right",
        KeyButton.Get(Keys.D)
    );

    public static void ReadFromConfig() {
        foreach (var bind in ClientConfig.instance.keybinds) {
            if (!binds.ContainsKey(bind.Key))
                continue;
            var bindToSet = binds[bind.Key];
            bindToSet.ReadButtonString(bind.Value);
        }
    }

    public static void WriteToConfig() {
        Dictionary<string, string[]> keybinds = new();
        
        foreach (var bind in binds) {
            keybinds[bind.Key] = bind.Value.GetButtonString();
        }

        ClientConfig.instance.keybinds = keybinds;
    }
}
