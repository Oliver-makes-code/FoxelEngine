using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Foxel.Common.Util;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content.Components;

public interface Component<TComponent> where TComponent : Component<TComponent> {
    public static abstract Codec<Variant<ResourceKey, TComponent>> GetVariantCodec();
}

public interface ComponentHolder<TComponentParent> where TComponentParent : Component<TComponentParent> {
    public bool TryGetComponent<TComponent>(ResourceKey key, [NotNullWhen(true)] out TComponent? component) where TComponent : class, TComponentParent;
}

public class ImmutableComponentHolder<TComponentParent>(Dictionary<ResourceKey, TComponentParent> components) : ComponentHolder<TComponentParent> where TComponentParent : Component<TComponentParent> {
    public static readonly Codec<ImmutableComponentHolder<TComponentParent>> Codec = new HolderCodec();
    private readonly ReadOnlyDictionary<ResourceKey, TComponentParent> Components = new(components);

    public bool TryGetComponent<TComponent>(ResourceKey key, [NotNullWhen(true)] out TComponent? component) where TComponent : class, TComponentParent {
        if (Components.TryGetValue(key, out var maybe) && Conditions.TryCast(maybe, out component))
            return true;
        component = null;
        return false;
    }

    private record HolderCodec : Codec<ImmutableComponentHolder<TComponentParent>> {
        public override ImmutableComponentHolder<TComponentParent> ReadGeneric(DataReader reader) {
            var components = TComponentParent.GetVariantCodec().Array().ReadGeneric(reader);
            var dict = new Dictionary<ResourceKey, TComponentParent>();
            foreach (var component in components)
                dict[component.type] = component.value;
            return new(dict);
        }
        public override void WriteGeneric(DataWriter writer, ImmutableComponentHolder<TComponentParent> value) {
            using var arr = writer.Array(value.Components.Count);
            foreach (var key in value.Components.Keys)
                TComponentParent.GetVariantCodec().WriteGeneric(arr.Value(), new(key, value.Components[key]));
        }
    }
}

public class MutableComponentHolder<TComponentParent>(ImmutableComponentHolder<TComponentParent> parent) : ComponentHolder<TComponentParent> where TComponentParent : Component<TComponentParent> {
    public readonly ImmutableComponentHolder<TComponentParent> Parent = parent;
    private readonly Dictionary<ResourceKey, TComponentParent> Components = [];

    public bool TryGetComponent<TComponent>(ResourceKey key, [NotNullWhen(true)] out TComponent? component) where TComponent : class, TComponentParent {
        if (Components.TryGetValue(key, out var maybe) && Conditions.TryCast(maybe, out component))
            return true;
        return Parent.TryGetComponent(key, out component);
    }

    public void SetComponent(ResourceKey key, TComponentParent component) {
        Components[key] = component;
    }
}
