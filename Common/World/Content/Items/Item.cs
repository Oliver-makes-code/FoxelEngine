using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Foxel.Common.Collision;
using Foxel.Common.Util;
using Foxel.Common.World.Content.Items.Components;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content.Items;

public class Item(ImmutableItemComponentHolder components) {
    public readonly ImmutableItemComponentHolder Components = components;

    public ItemStack NewStack()
        => new(this);

    public virtual void UseOnBlock(ref ItemStack stack, VoxelWorld world, BlockRaycastHit hit) {}
}

public struct ItemStack(Item item) {
    public readonly Item Item = item;
    public readonly MutableItemComponentHolder Components = new(item.Components);
    public uint count;

    public void UseOnBlock(VoxelWorld world, BlockRaycastHit hit) {
        Item.UseOnBlock(ref this, world, hit);
    }
}

public interface ItemComponentHolder {
    public bool TryGetComponent<TComponent>(ResourceKey key, [NotNullWhen(true)] out TComponent? component) where TComponent : class, ItemComponent;
}

public class ImmutableItemComponentHolder(Dictionary<ResourceKey, ItemComponent> components) : ItemComponentHolder {
    public static readonly Codec<ImmutableItemComponentHolder> Codec = new HolderCodec();
    private readonly ReadOnlyDictionary<ResourceKey, ItemComponent> Components = new(components);

    public bool TryGetComponent<TComponent>(ResourceKey key, [NotNullWhen(true)] out TComponent? component) where TComponent : class, ItemComponent {
        if (Components.TryGetValue(key, out var maybe) && Conditions.TryCast(maybe, out component))
            return true;
        component = null;
        return false;
    }

    private record HolderCodec : Codec<ImmutableItemComponentHolder> {
        public override ImmutableItemComponentHolder ReadGeneric(DataReader reader) {
            var components = ItemComponent.Codec.Array().ReadGeneric(reader);
            var dict = new Dictionary<ResourceKey, ItemComponent>();
            foreach (var component in components)
                dict[component.type] = component.value;
            return new(dict);
        }
        public override void WriteGeneric(DataWriter writer, ImmutableItemComponentHolder value) {
            using var arr = writer.Array(value.Components.Count);
            foreach (var key in value.Components.Keys)
                ItemComponent.Codec.WriteGeneric(arr.Value(), new(key, value.Components[key]));
        }
    }
}

public class MutableItemComponentHolder(ImmutableItemComponentHolder parent) : ItemComponentHolder {
    public readonly ImmutableItemComponentHolder Parent = parent;
    private readonly Dictionary<ResourceKey, ItemComponent> Components = [];

    public bool TryGetComponent<TComponent>(ResourceKey key, [NotNullWhen(true)] out TComponent? component) where TComponent : class, ItemComponent {
        if (Components.TryGetValue(key, out var maybe) && Conditions.TryCast(maybe, out component))
            return true;
        return Parent.TryGetComponent(key, out component);
    }

    public void SetComponent(ResourceKey key, ItemComponent component) {
        Components[key] = component;
    }
}
