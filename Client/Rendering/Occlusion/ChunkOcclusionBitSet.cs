using System;
using System.Runtime.CompilerServices;

namespace Foxel.Client.Rendering.Occlusion; 

/**
 * Determines which sides are visible from others in a chunk
 * Ex: UpDown means there is a visible pathway from the up face to the down face.
 */
[Flags]
public enum ChunkOcclusionBitSet : ushort {
    UpDown     = 0b000000000000001,
    UpNorth    = 0b000000000000010,
    UpSouth    = 0b000000000000100,
    UpEast     = 0b000000000001000,
    UpWest     = 0b000000000010000,
    DownNorth  = 0b000000000100000,
    DownSouth  = 0b000000001000000,
    DownEast   = 0b000000010000000,
    DownWest   = 0b000000100000000,
    NorthSouth = 0b000001000000000,
    NorthEast  = 0b000010000000000,
    NorthWest  = 0b000100000000000,
    SouthEast  = 0b001000000000000,
    SouthWest  = 0b010000000000000,
    EastWest   = 0b100000000000000,

    Up         = 0b000000000011111,
    Down       = 0b000000111100001,
    North      = 0b000111000100010,
    South      = 0b011001001000100,
    East       = 0b101010010001000,
    West       = 0b110100100010000,
    
    All        = 0b111111111111111,
    None       = 0b000000000000000,
}

public static class ChunkOcclusionBitSetExtension {
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Test(this ChunkOcclusionBitSet set, ChunkOcclusionBitSet direction)
        => (set ^ direction) == 0;
}
