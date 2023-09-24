using System;

namespace Voxel.Common.World;

public class Chunk {
    public static readonly Chunk Empty = new();

    readonly ushort[] data = new ushort[ushort.MaxValue];

    public Chunk() {}

    float StackedNoise(long seed, float x, float y, int octaves, float persistence) {
        float total = 0;
        float amplitude = 1;
        float frequency = 1;
        float maxValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            total += OpenSimplex2.Noise2(seed, x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }

    public void FillWithSimplexNoise(ChunkPos pos) {
        for (byte x = 0; x < 0b10_0000u; x++) {
            for (byte z = 0; z < 0b10_0000u; z++) {
                var noise = StackedNoise(0, (pos.x*32f + x)/128, (pos.z*32f + z)/128, 4, 0.4f);
                noise += 1;
                noise *= 32;
                var bin_noise = (byte)noise - pos.y*32;
                if (bin_noise > 32)
                    bin_noise = 32;
                for (byte y = 0; y < bin_noise; y++) {
                    this[false, x, y, z] = (ushort)0b0000_0000_0010_0000u;
                }
            }
        }
    }

    public ushort this[int idx] {
        get => data[idx];
        set => data[idx] = value;
    }

    public ushort this[ChunkBlockPos pos] {
        get => this[pos.Raw];
        set => this[pos.Raw] = value;
    }

    public ushort this[bool fluid, int x, int y, int z] {
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

    public int Raw { get; private set; } = 0;

    public ChunkBlockPos(int raw) { Raw = raw; }
    public ChunkBlockPos(bool isFluid, int x, int y, int z) : this(GetRawFrom(isFluid, x, y, z)) {}

    public static int GetRawFrom(bool isFluid, int x, int y, int z) {
        int val = isFluid ? TEST_FLUID : 0;
        val += (x & TEST_Z) << SHIFT_X;
        val += (y & TEST_Z) << SHIFT_Y;
        val += z & TEST_Z;
        return val;
    }

    public bool IsFluid {
        readonly get => (Raw & TEST_FLUID) != 0;
        set => Raw = ((value ? 1 : 0) << SHIFT_FLUID) | (Raw & SET_FLUID);
    }

    public byte X {
        readonly get => (byte)((Raw & TEST_X) >> SHIFT_X);
        set => Raw = ((value & TEST_Z) << SHIFT_X) | (Raw & SET_X);
    }

    public byte Y {
        readonly get => (byte)((Raw & TEST_Y) >> SHIFT_Y);
        set => Raw = ((value & TEST_Z) << SHIFT_Y) | (Raw & SET_Y);
    }

    public byte Z {
        readonly get => (byte)(Raw & TEST_Z);
        set => Raw = (value & TEST_Z) | (Raw & SET_Z);
    }
}
