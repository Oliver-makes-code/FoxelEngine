namespace Foxel.Common.World.Content.Blocks.State;

public interface BlockProperty {
    public byte GetPropertyCount();
    public string GetName();
}

public interface BlockProperty<TValue> : BlockProperty where TValue : struct  {
    public TValue GetValue(byte index);

    public byte GetIndex(TValue value);

    public bool ValidIndex(byte index);

    public bool ValidValue(TValue value);
}
