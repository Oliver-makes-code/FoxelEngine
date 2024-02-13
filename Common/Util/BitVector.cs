namespace Voxel.Common.Util;

public class BitVector {
    private static readonly byte[] Table = [
        0b00000001,
        0b00000010,
        0b00000100,
        0b00001000,
        0b00010000,
        0b00100000,
        0b01000000,
        0b10000000,
    ];
    private readonly byte[] Bytes;

    public BitVector(int size) {
        // Keep a buffer of one byte Just In Case™
        Bytes = new byte[(size >> 3) + 1];
        Clear();
    }

    /// We aren't allowing setting because that's a bit more complex and should be done knowingly.
    public bool this[int index] {
        get => Get(index);
        set {
            if (value)
                Set(index);
            else
                Unset(index);
        }
    }

    public bool Get(int index) {
        byte b = Bytes[index >> 3];
        int byteIdx = index & 0b111;
        return (b & Table[byteIdx]) != 0;
    }

    public void Set(int index) {
        int byteIdx = index & 0b111;
        Bytes[index >> 3] |= Table[byteIdx];
    }

    public void Unset(int index) {
        int byteIdx = index & 0b111;
        Bytes[index >> 3] &= (byte)(Table[byteIdx] ^ 0b11111111);
    }

    public void Clear() {
        for (int i = 0; i < Bytes.Length; i++) {
            Bytes[i] = 0;
        }
    }
}
