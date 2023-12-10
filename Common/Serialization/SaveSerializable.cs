using PeterO.Cbor;

namespace Common.Serialization; 

public abstract class SaveSerializable<T> {
    public abstract void Load(T data);
    public abstract T Save();

    public CBORObject SaveToCbor()
        => CBORObject.FromObject(Save());
    public void LoadFromCbor(CBORObject data)
        => Load(data.ToObject<T>());
}
