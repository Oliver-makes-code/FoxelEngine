using System;
using System.Collections.Generic;
using System.Linq;

namespace Voxel.Client.Keybinding;

public class Keybind {
    public readonly Button[] defaultButtons;
    public List<Button> currentButtons;
    public bool justPressed { get; private set; }

    public bool justReleased { get; private set; }

    public bool isPressed { get; private set; }

    public float strength {
        get {
            float max = 0;
            foreach (var button in currentButtons)
                max = MathF.Max(max, button.strength);
            return max;
        }
    }

    public Keybind(string name, params Button[] defaultButtons) {
        this.defaultButtons = defaultButtons;
        currentButtons = defaultButtons.ToList();

        if (Keybinds.binds.ContainsKey(name))
            throw new ArgumentException("Keybinding '"+name+"' already exists.");
        
        Keybinds.binds[name] = this;
    }

    public void Poll() {
        if (currentButtons.Any(button => button.isPressed)) {
            justReleased = false;
            justPressed = !isPressed;
            isPressed = true;
            return;
        }
        justPressed = false;
        justReleased = isPressed;
        isPressed = false;
    }

    public string[] GetButtonString() {
        var output = new string[currentButtons.Count];
        for (int i = 0; i < currentButtons.Count; i++) {
            output[i] = currentButtons[i].ToString();
        }
        return output;
    }

    public void ReadButtonString(string[] buttons) {
        currentButtons = new();
        foreach (var button in buttons) {
            var curr = Button.FromString(button);
            if (curr == null)
                continue;
            currentButtons.Add(curr);
        }
    }
}