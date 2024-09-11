using System;
using GlmSharp;

namespace Foxel.Client.Input;

public static class ActionGroups {
    public static readonly ActionGroup<vec2> Movement = new Vec2ActionGroup(
        new Vec2ActionGroup.Keys(
            North: KeyboardMouseAction.Keyboard.W,
            South: KeyboardMouseAction.Keyboard.S,
            East:  KeyboardMouseAction.Keyboard.D,
            West:  KeyboardMouseAction.Keyboard.A
        ),
        GamepadAction.Stick.Left
    );

    public static readonly ActionGroup<vec2> Look = new Vec2ActionGroup(
        new Vec2ActionGroup.Mouse(KeyboardMouseAction.MouseAxis),
        GamepadAction.Stick.Right
    );

    public static readonly ActionGroup<bool> Jump = new BoolActionGroup(
        KeyboardMouseAction.Keyboard.Space,
        GamepadAction.Button.South
    );

    public static readonly ActionGroup<bool> Ssao = new BoolActionGroup(
        KeyboardMouseAction.Keyboard.K,
        GamepadAction.Button.Back
    );

    public static readonly ActionGroup<bool> Use = new BoolActionGroup(
        KeyboardMouseAction.Mouse.Right,
        GamepadAction.DigitalTrigger.Left
    );

    public static readonly ActionGroup<bool> Attack = new BoolActionGroup(
        KeyboardMouseAction.Mouse.Left,
        GamepadAction.DigitalTrigger.Right
    );

    public static readonly ActionGroup<bool> NextSlot = new BoolActionGroup(
        KeyboardMouseAction.MouseWheelDown,
        GamepadAction.Button.RightBumper
    );

    public static readonly ActionGroup<bool> LastSlot = new BoolActionGroup(
        KeyboardMouseAction.MouseWheelUp,
        GamepadAction.Button.LeftBumper
    );

    public static readonly ActionGroup<bool> Pause = new BoolActionGroup(
        KeyboardMouseAction.Keyboard.Escape,
        GamepadAction.Button.Start
    );

    public static readonly ActionGroup<bool> Refresh = new BoolActionGroup(
        KeyboardMouseAction.Keyboard.F5,
        GamepadAction.Button.Back
    );

    public static readonly ActionGroup<bool> Screenshot = new BoolActionGroup(
        KeyboardMouseAction.Keyboard.F2,
        GamepadAction.Button.Guide
    );
}
