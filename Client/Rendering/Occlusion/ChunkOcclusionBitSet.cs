using System;
using System.Runtime.CompilerServices;

namespace Voxel.Client.Rendering.Occlusion; 

/**
 * Determines which sides are visible from others in a chunk
 * Ex: UpDown means there is a visible pathway from the up face to the down face.
 */
[Flags]
public enum ChunkOcclusionBitSet : ushort {
    UpDown     = 0b0000000000000001,
    UpNorth    = 0b0000000000000010,
    UpSouth    = 0b0000000000000100,
    UpEast     = 0b0000000000001000,
    UpWest     = 0b0000000000010000,
    DownNorth  = 0b0000000000100000,
    DownSouth  = 0b0000000001000000,
    DownEast   = 0b0000000010000000,
    DownWest   = 0b0000000100000000,
    NorthSouth = 0b0000001000000000,
    NorthEast  = 0b0000010000000000,
    NorthWest  = 0b0000100000000000,
    SouthEast  = 0b0001000000000000,
    SouthWest  = 0b0010000000000000,
    EastWest   = 0b0100000000000000,
    
    Up         = 0b0000000000011111,
    Down       = 0b0000000111100001,
    North      = 0b0000111000100010,
    South      = 0b0011001001000100,
    East       = 0b0101010010001000,
    West       = 0b0110100100010000,
    
    All        = 0b0111111111111111,
}

public static class ChunkOcclusionBitSetExtension {
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Test(this ChunkOcclusionBitSet set, ChunkOcclusionBitSet direction)
        => (set ^ direction) == 0;
}
