namespace Foxel.Common.World.Blocks;

public interface BlockProperty {
    public int GetCount();
    public string GetName();
}

public interface BlockProperty<TValue> : BlockProperty where TValue : struct {
    public TValue GetValue(int idx);
}

public readonly record struct EnumBlockProperty<TEnum>(string Name) : BlockProperty<TEnum> where TEnum : struct, Enum {
    public readonly int GetCount()
        => Enum.GetValues<TEnum>().Length;

    public readonly string GetName()
        => Name;

    public readonly TEnum GetValue(int idx)
        => Enum.GetValues<TEnum>()[idx];
}

public readonly record struct BoolBlockProperty(string Name) : BlockProperty<bool> {
    public readonly int GetCount()
        => 2;

    public readonly string GetName()
        => Name;

    public readonly bool GetValue(int idx)
        => idx switch {
            0 => false,
            1 => true,
            _ => throw new IndexOutOfRangeException(nameof(idx)),
        };
}

public readonly record struct IntRangeBlockProperty(string Name, int Min, int Max) : BlockProperty<int> {
    public int GetCount()
        => Max - Min;

    public string GetName()
        => Name;

    public int GetValue(int idx) {
        int value = idx + Min;
        if (value > Max)
            throw new IndexOutOfRangeException(nameof(idx));
        return value;
    }
}
