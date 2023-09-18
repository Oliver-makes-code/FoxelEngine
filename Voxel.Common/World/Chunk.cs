namespace Voxel.Common.World;

public struct Chunk {
    readonly ushort[] data;

    public Chunk() {
        data = new ushort[ushort.MaxValue];

        for (byte y = 0; y < 0b1_0000u; y++) {
            for (byte x = 0; x < 0b10_0000u; x++) {
                for (byte z = 0; z < 0b10_0000u; z++) {
                    float min = (float)y/32;
                    if (Random.Shared.NextSingle() > min)
                        this[false, x, y, z] = (ushort)0b1000_0000_0001_0000u;
                }
            }
        }
    }

    public readonly ushort this[ushort idx] {
        get => data[idx];
        set => data[idx] = value;
    }

    public readonly ushort this[ChunkPos pos] {
        get => this[pos.Raw];
        set => this[pos.Raw] = value;
    }

    public readonly ushort this[bool fluid, byte x, byte y, byte z] {
        get => this[new ChunkPos(fluid, x, y, z)];
        set => this[new ChunkPos(fluid, x, y, z)] = value;
    }
}

public struct ChunkPos {
    public ushort Raw { get; private set; } = 0;

    public ChunkPos(ushort raw) { Raw = raw; }
    public ChunkPos(bool isFluid, byte x, byte y, byte z) {
        IsFluid = isFluid;
        X = x;
        Y = y;
        Z = z;
    }

    public bool IsFluid {
        readonly get => (Raw & 0b1000000000000000) != 0;
        set => Raw = (ushort)(((value ? 1 : 0) << 15) | (Raw & 0b0111111111111111));
    }

    public byte X {
        readonly get => (byte)(Raw & 0b0111110000000000 >> 10);
        set => Raw = (ushort)((value << 10) | (Raw & 0b1000001111111111));
    }

    public byte Y {
        readonly get => (byte)(Raw & 0b0000001111100000 >> 5);
        set => Raw = (ushort)((value << 5) | (Raw & 0b1111110000011111));
    }

    public byte Z {
        readonly get => (byte)(Raw & 0b0000000000011111);
        set => Raw = (ushort)(value | (Raw & 0b1111111111100000));
    }
}
