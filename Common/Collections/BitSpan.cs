namespace Foxel.Common.Collections;

public readonly record struct BitSpan(
    byte Offset,
    byte Length
) {
    private uint Mask
        => (1u << Length) - 1;

    public uint Get(uint number)
        => (number >> Offset) & Mask;

    public uint Set(uint number, uint value)
        => (number & ~(Mask << Offset)) | ((value & Mask) << Offset);
}
