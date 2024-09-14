using Foxel.Common.Network.Packets;
using Foxel.Common.World.Content.Items;
using Foxel.Common.World.Content.Items.Components;
using Foxel.Common.World.Content.Packets;
using Foxel.Common.World.Content.Entities;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;
using Foxel.Common.World.Content.Blocks;
using Foxel.Common.World.Content.Noise;

namespace Foxel.Common.World.Content;

public static class ContentStores {
    public static readonly ContentStore<RecordCodec<Item>> ItemCodecs = new(ContentStage.Static, "ItemCodecs");
    public static readonly ContentStore<RecordCodec<Block>> BlockCodecs = new(ContentStage.Static, "BlockCodecs");
    public static readonly ContentStore<RecordCodec<ItemComponent>> ItemComponentCodecs = new(ContentStage.Static, "ItemCodecs");
    public static readonly ContentStore<Codec<Packet>> PacketCodecs = new(ContentStage.Static, "PacketCodecs");
    public static readonly ContentStore<Codec<Entity>> Entitycodecs = new(ContentStage.Static, "EntitiyCodecs");

    public static readonly ContentStore<Item> Items = new(ContentStage.Dynamic, "Items");
    public static readonly ContentStore<Block> Blocks = new(ContentStage.Dynamic, "Blocks");
    public static readonly ContentStore<NoiseMap> NoiseMaps = new(ContentStage.Dynamic, "NoiseMaps");

    public static void InitStaticStores() {
        ItemStore.RegisterStaticContent();
        BlockStore.RegisterStaticContent();
        PacketStore.RegisterStaticContent();
        EntityStore.RegisterStaticContent();
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

    public IEnumerable<ResourceKey> Keys() {
        foreach (var (key, _) in IdToValue)
            yield return key;
    }

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
