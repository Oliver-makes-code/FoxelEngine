using System;
using System.Collections.Generic;
using GlmSharp;
using Veldrid;
using Voxel.Core.Input;

namespace Voxel.Client.Input;

public abstract record KeyboardMouseAction<TOutput> : KeyboardMouseAction {
    internal KeyboardMouseAction() {}

    public abstract TOutput GetOutput(InputManager manager);
}

public abstract record KeyboardMouseAction {
    public static readonly KeyboardMouseAction<vec2> MouseAxis = new Simple<vec2>(manager => manager.MouseDelta);

    private static readonly List<KeyboardMouseAction> Values = [];
    internal KeyboardMouseAction() {
        Values.Add(this);
    }

    public record Mouse : KeyboardMouseAction<bool> {
        public static readonly Mouse Left = new(MouseButton.Left);
        public static readonly Mouse Middle = new(MouseButton.Middle);
        public static readonly Mouse Right = new(MouseButton.Right);
        public static readonly Mouse Button1 = new(MouseButton.Button1);
        public static readonly Mouse Button2 = new(MouseButton.Button2);
        public static readonly Mouse Button3 = new(MouseButton.Button3);
        public static readonly Mouse Button4 = new(MouseButton.Button4);
        public static readonly Mouse Button5 = new(MouseButton.Button5);
        public static readonly Mouse Button6 = new(MouseButton.Button6);
        public static readonly Mouse Button7 = new(MouseButton.Button7);
        public static readonly Mouse Button8 = new(MouseButton.Button8);
        public static readonly Mouse Button9 = new(MouseButton.Button9);

        private readonly MouseButton Button;

        private Mouse(MouseButton button) {
            Button = button;
        }

        public override bool GetOutput(InputManager manager)
            => manager.IsMouseButtonPressed(Button);
    }

    public record Keyboard : KeyboardMouseAction<bool> {
        public static readonly Keyboard ShiftLeft = new(Key.ShiftLeft);
        public static readonly Keyboard LShift = new(Key.LShift);
        public static readonly Keyboard ShiftRight = new(Key.ShiftRight);
        public static readonly Keyboard RShift = new(Key.RShift);
        public static readonly Keyboard ControlLeft = new(Key.ControlLeft);
        public static readonly Keyboard LControl = new(Key.LControl);
        public static readonly Keyboard ControlRight = new(Key.ControlRight);
        public static readonly Keyboard RControl = new(Key.RControl);
        public static readonly Keyboard AltLeft = new(Key.AltLeft);
        public static readonly Keyboard LAlt = new(Key.LAlt);
        public static readonly Keyboard AltRight = new(Key.AltRight);
        public static readonly Keyboard RAlt = new(Key.RAlt);
        public static readonly Keyboard WinLeft = new(Key.WinLeft);
        public static readonly Keyboard LWin = new(Key.LWin);
        public static readonly Keyboard WinRight = new(Key.WinRight);
        public static readonly Keyboard RWin = new(Key.RWin);
        public static readonly Keyboard Menu = new(Key.Menu);
        public static readonly Keyboard F1 = new(Key.F1);
        public static readonly Keyboard F2 = new(Key.F2);
        public static readonly Keyboard F3 = new(Key.F3);
        public static readonly Keyboard F4 = new(Key.F4);
        public static readonly Keyboard F5 = new(Key.F5);
        public static readonly Keyboard F6 = new(Key.F6);
        public static readonly Keyboard F7 = new(Key.F7);
        public static readonly Keyboard F8 = new(Key.F8);
        public static readonly Keyboard F9 = new(Key.F9);
        public static readonly Keyboard F10 = new(Key.F10);
        public static readonly Keyboard F11 = new(Key.F11);
        public static readonly Keyboard F12 = new(Key.F12);
        public static readonly Keyboard F13 = new(Key.F13);
        public static readonly Keyboard F14 = new(Key.F14);
        public static readonly Keyboard F15 = new(Key.F15);
        public static readonly Keyboard F16 = new(Key.F16);
        public static readonly Keyboard F17 = new(Key.F17);
        public static readonly Keyboard F18 = new(Key.F18);
        public static readonly Keyboard F19 = new(Key.F19);
        public static readonly Keyboard F20 = new(Key.F20);
        public static readonly Keyboard F21 = new(Key.F21);
        public static readonly Keyboard F22 = new(Key.F22);
        public static readonly Keyboard F23 = new(Key.F23);
        public static readonly Keyboard F24 = new(Key.F24);
        public static readonly Keyboard F25 = new(Key.F25);
        public static readonly Keyboard F26 = new(Key.F26);
        public static readonly Keyboard F27 = new(Key.F27);
        public static readonly Keyboard F28 = new(Key.F28);
        public static readonly Keyboard F29 = new(Key.F29);
        public static readonly Keyboard F30 = new(Key.F30);
        public static readonly Keyboard F31 = new(Key.F31);
        public static readonly Keyboard F32 = new(Key.F32);
        public static readonly Keyboard F33 = new(Key.F33);
        public static readonly Keyboard F34 = new(Key.F34);
        public static readonly Keyboard F35 = new(Key.F35);
        public static readonly Keyboard Up = new(Key.Up);
        public static readonly Keyboard Down = new(Key.Down);
        public static readonly Keyboard Left = new(Key.Left);
        public static readonly Keyboard Right = new(Key.Right);
        public static readonly Keyboard Enter = new(Key.Enter);
        public static readonly Keyboard Escape = new(Key.Escape);
        public static readonly Keyboard Space = new(Key.Space);
        public static readonly Keyboard Tab = new(Key.Tab);
        public static readonly Keyboard BackSpace = new(Key.BackSpace);
        public static readonly Keyboard Back = new(Key.Back);
        public static readonly Keyboard Insert = new(Key.Insert);
        public static readonly Keyboard Delete = new(Key.Delete);
        public static readonly Keyboard PageUp = new(Key.PageUp);
        public static readonly Keyboard PageDown = new(Key.PageDown);
        public static readonly Keyboard Home = new(Key.Home);
        public static readonly Keyboard End = new(Key.End);
        public static readonly Keyboard CapsLock = new(Key.CapsLock);
        public static readonly Keyboard ScrollLock = new(Key.ScrollLock);
        public static readonly Keyboard PrintScreen = new(Key.PrintScreen);
        public static readonly Keyboard Pause = new(Key.Pause);
        public static readonly Keyboard NumLock = new(Key.NumLock);
        public static readonly Keyboard Clear = new(Key.Clear);
        public static readonly Keyboard Sleep = new(Key.Sleep);
        public static readonly Keyboard Keypad0 = new(Key.Keypad0);
        public static readonly Keyboard Keypad1 = new(Key.Keypad1);
        public static readonly Keyboard Keypad2 = new(Key.Keypad2);
        public static readonly Keyboard Keypad3 = new(Key.Keypad3);
        public static readonly Keyboard Keypad4 = new(Key.Keypad4);
        public static readonly Keyboard Keypad5 = new(Key.Keypad5);
        public static readonly Keyboard Keypad6 = new(Key.Keypad6);
        public static readonly Keyboard Keypad7 = new(Key.Keypad7);
        public static readonly Keyboard Keypad8 = new(Key.Keypad8);
        public static readonly Keyboard Keypad9 = new(Key.Keypad9);
        public static readonly Keyboard KeypadDivide = new(Key.KeypadDivide);
        public static readonly Keyboard KeypadMultiply = new(Key.KeypadMultiply);
        public static readonly Keyboard KeypadSubtract = new(Key.KeypadSubtract);
        public static readonly Keyboard KeypadMinus = new(Key.KeypadMinus);
        public static readonly Keyboard KeypadAdd = new(Key.KeypadAdd);
        public static readonly Keyboard KeypadPlus = new(Key.KeypadPlus);
        public static readonly Keyboard KeypadDecimal = new(Key.KeypadDecimal);
        public static readonly Keyboard KeypadPeriod = new(Key.KeypadPeriod);
        public static readonly Keyboard KeypadEnter = new(Key.KeypadEnter);
        public static readonly Keyboard A = new(Key.A);
        public static readonly Keyboard B = new(Key.B);
        public static readonly Keyboard C = new(Key.C);
        public static readonly Keyboard D = new(Key.D);
        public static readonly Keyboard E = new(Key.E);
        public static readonly Keyboard F = new(Key.F);
        public static readonly Keyboard G = new(Key.G);
        public static readonly Keyboard H = new(Key.H);
        public static readonly Keyboard I = new(Key.I);
        public static readonly Keyboard J = new(Key.J);
        public static readonly Keyboard K = new(Key.K);
        public static readonly Keyboard L = new(Key.L);
        public static readonly Keyboard M = new(Key.M);
        public static readonly Keyboard N = new(Key.N);
        public static readonly Keyboard O = new(Key.O);
        public static readonly Keyboard P = new(Key.P);
        public static readonly Keyboard Q = new(Key.Q);
        public static readonly Keyboard R = new(Key.R);
        public static readonly Keyboard S = new(Key.S);
        public static readonly Keyboard T = new(Key.T);
        public static readonly Keyboard U = new(Key.U);
        public static readonly Keyboard V = new(Key.V);
        public static readonly Keyboard W = new(Key.W);
        public static readonly Keyboard X = new(Key.X);
        public static readonly Keyboard Y = new(Key.Y);
        public static readonly Keyboard Z = new(Key.Z);
        public static readonly Keyboard Number0 = new(Key.Number0);
        public static readonly Keyboard Number1 = new(Key.Number1);
        public static readonly Keyboard Number2 = new(Key.Number2);
        public static readonly Keyboard Number3 = new(Key.Number3);
        public static readonly Keyboard Number4 = new(Key.Number4);
        public static readonly Keyboard Number5 = new(Key.Number5);
        public static readonly Keyboard Number6 = new(Key.Number6);
        public static readonly Keyboard Number7 = new(Key.Number7);
        public static readonly Keyboard Number8 = new(Key.Number8);
        public static readonly Keyboard Number9 = new(Key.Number9);
        public static readonly Keyboard Tilde = new(Key.Tilde);
        public static readonly Keyboard Grave = new(Key.Grave);
        public static readonly Keyboard Minus = new(Key.Minus);
        public static readonly Keyboard Plus = new(Key.Plus);
        public static readonly Keyboard BracketLeft = new(Key.BracketLeft);
        public static readonly Keyboard LBracket = new(Key.LBracket);
        public static readonly Keyboard BracketRight = new(Key.BracketRight);
        public static readonly Keyboard RBracket = new(Key.RBracket);
        public static readonly Keyboard Semicolon = new(Key.Semicolon);
        public static readonly Keyboard Quote = new(Key.Quote);
        public static readonly Keyboard Comma = new(Key.Comma);
        public static readonly Keyboard Period = new(Key.Period);
        public static readonly Keyboard Slash = new(Key.Slash);
        public static readonly Keyboard BackSlash = new(Key.BackSlash);
        public static readonly Keyboard NonUSBackSlash = new(Key.NonUSBackSlash);

        private readonly Key Key;

        public Keyboard(Key key) {
            Key = key;
        }

        public override bool GetOutput(InputManager manager)
            => manager.IsKeyPressed(Key);
    }

    private record Simple<TOutput>(Func<InputManager, TOutput> Func) : KeyboardMouseAction<TOutput> {
        public override TOutput GetOutput(InputManager manager)
            => Func(manager);
    }
}
