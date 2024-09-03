using Foxel.Common.World.Content.Items.Components;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content.Items;

public static class ItemStore {
    public static class ItemCodecs {
        public static readonly RecordCodec<Item> Basic = RecordCodec<Item>.Create(
            ImmutableItemComponentHolder.Codec.NullableField<ImmutableItemComponentHolder, Item>("components", it => it.Components),
            (it) => new(it ?? new([]))
        );
        public static readonly RecordCodec<Item> BlockItem = RecordCodec<Item>.Create(
            ResourceKey.Codec.Field<Item>("block", it => ((BlockItem)it).Block),
            ImmutableItemComponentHolder.Codec.NullableField<ImmutableItemComponentHolder, Item>("components", it => it.Components),
            (block, components) => new BlockItem(block, components ?? new([]))
        );
    }

    public static class ComponentCodecs {
        public static readonly RecordCodec<ItemComponent> StackSize = RecordCodec<ItemComponent>.Create(
            Codecs.UInt.Field<ItemComponent>("max_stack_size", it => ((StackSizeItemComponent)it).maxStackSize),
            (maxStackSize) => new StackSizeItemComponent(maxStackSize)
        );
    }

    public static class Items {
        public static readonly ContentReference<Item> Empty = new ContentReference<Item>(ContentStores.Items, new("empty"));
        public static readonly ContentReference<Item> GrassBlock = new ContentReference<Item>(ContentStores.Items, new("block/grass"));
        public static readonly ContentReference<Item> DirtBlock = new ContentReference<Item>(ContentStores.Items, new("block/dirt"));
        public static readonly ContentReference<Item> StoneBlock = new ContentReference<Item>(ContentStores.Items, new("block/stone"));
        public static readonly ContentReference<Item> CobblestoneBlock = new ContentReference<Item>(ContentStores.Items, new("block/cobblestone"));
    }

    private static void RegisterItem(ResourceKey key, RecordCodec<Item> codec) {
        ContentStores.ItemCodecs.Register(key, codec);
    }
    private static void RegisterComponent(ResourceKey key, RecordCodec<ItemComponent> codec) {
        ContentStores.ItemComponentCodecs.Register(key, codec);
    }

    internal static void RegisterStaticContent() {
        RegisterItem(new("basic"), ItemCodecs.Basic);
        RegisterItem(new("block_item"), ItemCodecs.BlockItem);
        
        RegisterComponent(StackSizeItemComponent.Key, ComponentCodecs.StackSize);
    }
}
