using Foxel.Common.World.Content.Components;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content.Items.Components;

public abstract class ItemComponent : Component<ItemComponent> {
    public static readonly Codec<Variant<ResourceKey, ItemComponent>> Codec = new RecordVariantCodec<ResourceKey, ItemComponent>(
        ResourceKey.Codec,
        ContentStores.ItemComponentCodecs.GetValue
    );

    public static Codec<Variant<ResourceKey, ItemComponent>> GetVariantCodec()
        => Codec;

    public abstract RecordCodec<ItemComponent> GetCodec();
}
