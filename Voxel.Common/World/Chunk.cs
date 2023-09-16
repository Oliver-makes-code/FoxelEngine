namespace Voxel.Common.World;

public struct Chunk {
    readonly ushort[] data;

    public Chunk() {
        data = new ushort[ushort.MaxValue];

        for (byte y = 0; y < 0b10_0000u; y++) {
            for (byte x = 0; x < 0b10_0000u; x++) {
                for (byte z = 0; z < 0b10_0000u; z++) {
                    float min = ((float)y)/32;
                    if (Random.Shared.NextSingle() > min)
                        this[false, x, y, z] = (ushort)0b1000_0000_0001_0000u;
                }
            }
        }

        // this[false, 4, 1, 1] = 0;
    }

    public readonly ushort this[bool fluid, byte x, byte y, byte z] {
        get {
            if ((x|y|z) > 0b11111)
                return 0;
            uint idx = fluid ? 1u : 0u;
            idx *= 32;
            idx |= x&0b11111u;
            idx *= 32;
            idx |= y&0b11111u;
            idx *= 32;
            idx |= z&0b11111u;
            return data[idx];
        }
        set {
            if ((x|y|z) > 0b11111)
                return;
            uint idx = fluid ? 1u : 0u;
            idx *= 32;
            idx |= x&0b11111u;
            idx *= 32;
            idx |= y&0b11111u;
            idx *= 32;
            idx |= z&0b11111u;
            data[idx] = value;
        }
    }
}
