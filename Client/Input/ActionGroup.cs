using System.Collections.Generic;
using FastNoiseOO.Generators;
using GlmSharp;
using Voxel.Core.Input;

namespace Voxel.Client.Input;

public abstract class ActionGroup {
    private static readonly List<ActionGroup> Values = [];

    public ActionGroup() {
        Values.Add(this);
    }

    public static void UpdateAll(InputManager manager) {
        foreach (var value in Values)
            value.Update(manager);
    }

    public abstract void Update(InputManager manager);
}

public abstract class ActionGroup<TOutput> : ActionGroup {
    public abstract TOutput GetValue();

    public abstract bool WasUpdated();
}

public static class ActionGroupExtensions {
    public static bool WasJustPressed(this ActionGroup<bool> group)
        => group.WasUpdated() && group.GetValue();

    public static bool WasJustReleased(this ActionGroup<bool> group)
        => group.WasUpdated() && !group.GetValue();
}

public class BoolActionGroup : ActionGroup<bool> {
    public readonly KeyboardMouseAction<bool> KeyboardDefault;
    public readonly GamepadAction<bool> GamepadDefault;
    public KeyboardMouseAction<bool> keyboard;
    public GamepadAction<bool> gamepad;
    private bool oldValue;
    private bool value = false;

    public BoolActionGroup(KeyboardMouseAction<bool> keyboard, GamepadAction<bool> gamepad) {
        KeyboardDefault = keyboard;
        this.keyboard = keyboard;
        GamepadDefault = gamepad;
        this.gamepad = gamepad;
    }

    public override bool GetValue()
        => value;
    
    public override void Update(InputManager manager) {
        oldValue = value;
        value = keyboard.GetOutput(manager) || gamepad.GetOutput(manager, 0);
    }
    public override bool WasUpdated()
        => oldValue != value;
}

public class Vec2ActionGroup : ActionGroup<vec2> {
    public readonly Action KeyboardDefault;
    public readonly GamepadAction<vec2> GamepadDefault;
    public Action keyboard;
    public GamepadAction<vec2> gamepad;
    private vec2 oldValue;
    private vec2 value;

    public Vec2ActionGroup(Action keyboard, GamepadAction<vec2> gamepad) {
        KeyboardDefault = keyboard;
        this.keyboard = keyboard;
        GamepadDefault = gamepad;
        this.gamepad = gamepad;
    }

    public override vec2 GetValue()
        => value;
    
    public override void Update(InputManager manager) {
        oldValue = value;
        value = keyboard.GetValue(manager);
        var g = gamepad.GetOutput(manager, 0);
        if (value.LengthSqr < g.LengthSqr)
            value = g;
    }

    public override bool WasUpdated()
        => oldValue != value;

    public record Keys(KeyboardMouseAction<bool> North, KeyboardMouseAction<bool> South, KeyboardMouseAction<bool> East, KeyboardMouseAction<bool> West) : Action {
        public static float Axis(KeyboardMouseAction<bool> pos, KeyboardMouseAction<bool> neg, InputManager manager)
            => (pos.GetOutput(manager) ? 1 : 0) - (neg.GetOutput(manager) ? 1 : 0);
        
        public override vec2 GetValue(InputManager manager)
            => new(Axis(East, West, manager), Axis(South, North, manager));
    }

    public record Mouse(KeyboardMouseAction<vec2> Value) : Action {
        public override vec2 GetValue(InputManager manager)
            => Value.GetOutput(manager);
    }

    public abstract record Action {
        internal Action() {}

        public abstract vec2 GetValue(InputManager manager);
    }
}
