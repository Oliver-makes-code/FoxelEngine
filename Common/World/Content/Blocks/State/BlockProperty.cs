namespace Foxel.Common.World.Content.Blocks.State;

public interface BlockProperty {
    public byte GetPropertyCount();

    public string GetName();

    public object GetValueObject(byte index);
    
    public byte GetIndexObject(object value);

    public bool ValidIndex(byte index);

    public bool ValidValueObject(object value);
}

public abstract record BlockProperty<TValue> : BlockProperty where TValue : struct  {
    public object GetValueObject(byte index)
        => GetValue(index);

    public byte GetIndexObject(object value)
        => GetIndex((TValue)value);

    public bool ValidValueObject(object value)
        => ValidValue((TValue)value);

    public abstract TValue GetValue(byte index);

    public abstract byte GetIndex(TValue value);

    public abstract bool ValidValue(TValue value);

    public abstract byte GetPropertyCount();

    public abstract string GetName();

    public abstract bool ValidIndex(byte index);
}
