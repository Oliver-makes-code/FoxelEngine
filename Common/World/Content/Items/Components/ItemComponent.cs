using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content.Items.Components;

public interface ItemComponent {
    public static readonly Codec<Variant<ResourceKey, ItemComponent>> Codec = new RecordVariantCodec<ResourceKey, ItemComponent>(
        ResourceKey.Codec,
        ContentStores.ItemComponentCodecs.GetValue
    );

    public RecordCodec<ItemComponent> GetCodec();
}
