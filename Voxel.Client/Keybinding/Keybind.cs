namespace Voxel.Client.Keybinding;

public class Keybind {
    public readonly Button[] defaultButtons;
    public List<Button> currentButtons;
    private bool wasPressed = false;

    public bool isPressed {
        get {
            foreach (var button in currentButtons)
                if (button.isPressed)
                    return true;
            return false;
        }
    }

    public float strength {
        get {
            float max = 0;
            foreach (var button in currentButtons)
                max = MathF.Max(max, button.strength);
            return max;
        }
    }

    public bool justPressed {
        get {
            if (isPressed && !wasPressed) {
                wasPressed = true;
                return true;
            }
            return false;
        }
    }

    public bool justReleased { 
        get {
            if (wasPressed && !isPressed) {
                wasPressed = false;
                return true;
            }
            return false;
        }
    }

    public Keybind(string name, params Button[] defaultButtons) {
        this.defaultButtons = defaultButtons;
        currentButtons = defaultButtons.ToList();

        if (Keybinds.binds.ContainsKey(name))
            throw new ArgumentException("Keybinding '"+name+"' already exists.");
        
        Keybinds.binds[name] = this;
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
