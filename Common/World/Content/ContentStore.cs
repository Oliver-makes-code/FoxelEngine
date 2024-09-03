using Foxel.Common.World.Content.Items;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content;

public static class ContentStores {
    public static readonly ContentStore<RecordCodec<Item>> ItemCodecs = new(ContentStage.Static, "ItemCodecs");
    public static readonly ContentStore<RecordCodec<ItemComponent>> ItemComponentCodecs = new(ContentStage.Static, "ItemCodecs");

    public static readonly ContentStore<Item> Items = new(ContentStage.Dynamic, "Items");

    public static void InitStaticStores() {
        ItemStore.RegisterStaticContent();
    }
}

public record ContentStore<TValue>(ContentStage Stage, string Name) where TValue : notnull {
    private readonly Dictionary<ResourceKey, (TValue, int)> KeyToValue = [];
    private readonly Dictionary<TValue, (ResourceKey, int)> ValueToKey = [];
    private readonly List<(ResourceKey, TValue)> IdToValue = [];
    public bool frozen { get; private set; } = false;

    public TValue GetValue(ResourceKey key)
        => KeyToValue[key].Item1;
    public TValue GetValue(int id)
        => IdToValue[id].Item2;
    public ResourceKey GetKey(TValue value)
        => ValueToKey[value].Item1;
    public ResourceKey GetKey(int id)
        => IdToValue[id].Item1;
    public int GetId(TValue value)
        => ValueToKey[value].Item2;
    public int GetId(ResourceKey key)
        => KeyToValue[key].Item2;

    public void Register(ResourceKey key, TValue value) {
        if (KeyToValue.ContainsKey(key))
            throw new ArgumentException($"ResourceKey {key} for store {Name} already registered.");
        if (ValueToKey.ContainsKey(value))
            throw new ArgumentException($"Value {value} for store {Name} already registered.");

        int id = IdToValue.Count;
        IdToValue.Add((key, value));
        KeyToValue[key] = (value, id);
        ValueToKey[value] = (key, id);
    }

    internal void Freeze() {
        frozen = true;
    }
    
    internal void Clear() {
        if (Stage != ContentStage.Dynamic)
            throw new($"Clear() called on static content store ({Name}).");
        IdToValue.Clear();
        KeyToValue.Clear();
        frozen = false;
    }
}

public record struct ContentReference<TValue>(ContentStore<TValue> Store, ResourceKey Key) where TValue : notnull {
    public readonly TValue Get()
        => Store.GetValue(Key);
}

public enum ContentStage {
    Static,
    Dynamic
}
