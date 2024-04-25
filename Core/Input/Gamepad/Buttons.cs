using System.Runtime.CompilerServices;
using Veldrid.Sdl2;

namespace Voxel.Core.Input.Gamepad; 

public enum GamepadButton : byte {
    Invalid = unchecked((byte)-1),
    South = 0,
    East,
    West,
    North,
    Back,
    Guide,
    Start,
    LeftStick,
    RightStick,
    LeftShoulder,
    RightShoulder,
    DPadUp,
    DPadDown,
    DPadLeft,
    DPadRight,
    Max
}

public enum GamepadAxis {
    Invalid = unchecked((byte)-1),
    LeftX = 0,
    LeftY,
    RightX,
    RightY,
    LeftTrigger,
    RightTrigger,
    Max,
}

public static class ButtonsExtensions {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GamepadAxis IntoWrapped(this SDL_GameControllerAxis axis)
        => (GamepadAxis) axis;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SDL_GameControllerAxis FromWrapped(this GamepadAxis axis)
        => (SDL_GameControllerAxis) axis;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GamepadButton IntoWrapped(this SDL_GameControllerButton axis)
        => (GamepadButton) axis;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SDL_GameControllerButton FromWrapped(this GamepadButton axis)
        => (SDL_GameControllerButton) axis;
}
