namespace Foxel.Common.World.Content.Blocks.State;

public sealed record IntRangeProperty(string Name, byte Min, byte Max) : BlockProperty<byte> {
    public override byte GetIndex(byte value) {
        if (!ValidValue(value))
            throw new Exception($"Value {value} out of range for IntRange({Name})");
        return (byte)(value - Min);
    }

    public override string GetName()
        => Name;
    
    public override byte GetPropertyCount()
        => (byte)(Max - Min);

    public override byte GetValue(byte index) {
        if (!ValidIndex(index))
            throw new Exception($"Index {index} out of range for IntRange({Name})");
        return (byte)(index + Min);
    }

    public override bool ValidIndex(byte index)
        => index <= GetPropertyCount();

    public override bool ValidValue(byte value)
        => ValidIndex((byte)(value - Min));
}
