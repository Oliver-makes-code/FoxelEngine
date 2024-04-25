using System;
using System.Collections.Generic;
using GlmSharp;
using Voxel.Core.Input;
using Voxel.Core.Input.Gamepad;

namespace Voxel.Client.Input;

public abstract record GamepadAction<TOutput> : GamepadAction {
    internal GamepadAction() {}

    public abstract TOutput GetOutput(InputManager manager, int index);
}

public abstract record GamepadAction {
    private static readonly List<GamepadAction> Values = [];
    internal GamepadAction() {
        Values.Add(this);
    }

    public record Trigger : GamepadAction<double> {
        public static readonly Trigger Left = new(GamepadAxis.LeftTrigger);
        public static readonly Trigger Right = new(GamepadAxis.RightTrigger);

        private readonly GamepadAxis Axis;

        private Trigger(GamepadAxis axis) {
            Axis = axis;
        }

        public override double GetOutput(InputManager manager, int index)
            => manager.GetAxisStrength(Axis, index);
    }

    public record Stick : GamepadAction<vec2> {
        public static readonly Stick Left = new(GamepadAxis.LeftX, GamepadAxis.LeftY, () => (ClientConfig.General.deadzoneLeft, ClientConfig.General.snapLeft));
        public static readonly Stick Right = new(GamepadAxis.RightX, GamepadAxis.RightY, () => (ClientConfig.General.deadzoneRight, ClientConfig.General.snapRight));

        private readonly GamepadAxis X;
        private readonly GamepadAxis Y;
        private readonly Func<(float, float)>  DeadzoneFunc;

        private Stick(GamepadAxis x, GamepadAxis y, Func<(float, float)> deadzoneFunc) {
            X = x;
            Y = y;
            DeadzoneFunc = deadzoneFunc;
        }

        public override vec2 GetOutput(InputManager manager, int index) {
            var vec = new vec2(manager.GetAxisStrength(X, index), manager.GetAxisStrength(Y, index));

            var (deadzone, snap) = DeadzoneFunc();

            for (int i = 0; i < 2; i++)
                if (Math.Abs(vec[i]) < snap)
                    vec[i] = 0;
            
            if (deadzone <= 0)
                return vec;
            if (deadzone > 1)
                return new(0);

            if (vec.LengthSqr < deadzone * deadzone)
                return new(0);

            vec -= (vec2) vec2.Sign(vec) * deadzone;
        
            vec *= 1 / (1 - deadzone);

            return vec;
        }
    }

    public record Button : GamepadAction<bool> {
        public static readonly Button North = new(GamepadButton.North);
        public static readonly Button South = new(GamepadButton.South);
        public static readonly Button East = new(GamepadButton.East);
        public static readonly Button West = new(GamepadButton.West);
        public static readonly Button Back = new(GamepadButton.Back);
        public static readonly Button Guide = new(GamepadButton.Guide);
        public static readonly Button Start = new(GamepadButton.Start);
        public static readonly Button LeftStick = new(GamepadButton.LeftStick);
        public static readonly Button RightStick = new(GamepadButton.RightStick);
        public static readonly Button LeftBumper = new(GamepadButton.LeftShoulder);
        public static readonly Button RightBumper = new(GamepadButton.RightShoulder);
        public static readonly Button DPadUp = new(GamepadButton.DPadUp);
        public static readonly Button DPadDown = new(GamepadButton.DPadDown);
        public static readonly Button DPadRight = new(GamepadButton.DPadRight);
        public static readonly Button DPadLeft = new(GamepadButton.DPadRight); 

        private readonly GamepadButton _Button;

        private Button(GamepadButton button) {
            _Button = button;
        }

        public override bool GetOutput(InputManager manager, int index)
            => manager.IsButtonPressed(_Button, index);
    }

    public record DirectionalButton : GamepadAction<vec2> {
        public static readonly DirectionalButton DPad = new(GamepadButton.DPadRight, GamepadButton.DPadLeft, GamepadButton.DPadUp, GamepadButton.DPadDown);
        public static readonly DirectionalButton Face = new(GamepadButton.East, GamepadButton.West, GamepadButton.North, GamepadButton.South);

        private readonly GamepadButton XPos;
        private readonly GamepadButton XNeg;
        private readonly GamepadButton YPos;
        private readonly GamepadButton YNeg;

        private DirectionalButton(GamepadButton xPos, GamepadButton xNeg, GamepadButton yPos, GamepadButton yNeg) {
            XPos = xPos;
            XNeg = xNeg;
            YPos = yPos;
            YNeg = yNeg;
        }

        public static float Axis(GamepadButton pos, GamepadButton neg, InputManager manager, int index)
            => (manager.IsButtonPressed(pos, index) ? 1 : 0) - (manager.IsButtonPressed(neg, index) ? 1 : 0);

        public override vec2 GetOutput(InputManager manager, int index)
            => new(Axis(XPos, XNeg, manager, index), Axis(YPos, YNeg, manager, index));
    }
}
