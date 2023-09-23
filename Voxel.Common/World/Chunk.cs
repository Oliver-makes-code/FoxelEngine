using System;

namespace Voxel.Common.World;

public class Chunk {
    public static readonly Chunk Empty = new();

    readonly ushort[] data = new ushort[ushort.MaxValue];

    public Chunk() {}

    public void FillWithRandomData() {
        for (byte y = 0; y < 0b10_0000u; y++) {
            for (byte x = 0; x < 0b10_0000u; x++) {
                for (byte z = 0; z < 0b10_0000u; z++) {
                    float min = (float)y/32;
                    if (Random.Shared.NextSingle() > min)
                        this[false, x, y, z] = (ushort)0b0000_0000_0001_0000u;
                }
            }
        }
    }

    public ushort this[ushort idx] {
        get => data[idx];
        set => data[idx] = value;
    }

    public ushort this[ChunkBlockPos pos] {
        get => this[pos.Raw];
        set => this[pos.Raw] = value;
    }

    public ushort this[bool fluid, byte x, byte y, byte z] {
        get => this[ChunkBlockPos.GetRawFrom(fluid, x, y, z)];
        set => this[ChunkBlockPos.GetRawFrom(fluid, x, y, z)] = value;
    }
}

public struct ChunkBlockPos {
    public const int TEST_FLUID = 0b1000000000000000;
    public const int SET_FLUID = 0b0111111111111111;
    public const int SHIFT_FLUID = 15;
    public const int TEST_X = 0b0111110000000000;
    public const int SET_X = 0b1000001111111111;
    public const int SHIFT_X = 10;
    public const int TEST_Y = 0b0000001111100000;
    public const int SET_Y = 0b1111110000011111;
    public const int SHIFT_Y = 5;
    public const int TEST_Z = 0b0000000000011111;
    public const int SET_Z = 0b1111111111100000;

    public ushort Raw { get; private set; } = 0;

    public ChunkBlockPos(ushort raw) { Raw = raw; }
    public ChunkBlockPos(bool isFluid, byte x, byte y, byte z) : this(GetRawFrom(isFluid, x, y, z)) {}
    public ChunkBlockPos(bool isFluid, int x, int y, int z) : this(isFluid, (byte)x, (byte)y, (byte)z) {}

    public static ushort GetRawFrom(bool isFluid, byte x, byte y, byte z) {
        int val = isFluid ? TEST_FLUID : 0;
        val += (x & 0b11111) << SHIFT_X;
        val += (y & 0b11111) << SHIFT_Y;
        val += z & 0b11111;
        return (ushort)val;
    }

    public bool IsFluid {
        readonly get => (Raw & TEST_FLUID) != 0;
        set => Raw = (ushort)(((value ? 1 : 0) << SHIFT_FLUID) | (Raw & SET_FLUID));
    }

    public byte X {
        readonly get => (byte)((Raw & TEST_X) >> SHIFT_X);
        set => Raw = (ushort)((value << SHIFT_X) | (Raw & SET_X));
    }

    public byte Y {
        readonly get => (byte)((Raw & TEST_Y) >> SHIFT_Y);
        set => Raw = (ushort)((value << SHIFT_Y) | (Raw & SET_Y));
    }

    public byte Z {
        readonly get => (byte)(Raw & TEST_Z);
        set => Raw = (ushort)(value | (Raw & SET_Z));
    }
}
