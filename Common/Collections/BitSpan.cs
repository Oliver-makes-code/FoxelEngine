namespace Foxel.Common.Collections;

public readonly record struct BitSpan(
    byte Offset,
    byte Length
) {
    private int Mask
        => (1 << Length) - 1;

    public int Get(int number)
        => (number >> Offset) & Mask;

    public int Set(int number, int value)
        => (number & ~(Mask << Offset)) | ((value & Mask) << Offset);
}
