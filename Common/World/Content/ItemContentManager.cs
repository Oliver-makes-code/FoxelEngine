using Foxel.Common.Server;
using Foxel.Common.World.Content.Items;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content;

public class ItemContentManager() : ServerContentManager<Variant<ResourceKey, Item>, Item>(Codec, ContentStores.Items) {
    public static readonly Codec<Variant<ResourceKey, Item>> Codec = new RecordVariantCodec<ResourceKey, Item>(
        ResourceKey.Codec,
        ContentStores.ItemCodecs.GetValue
    );

    public override string ContentDir()
        => "items";

    public override Item Load(ResourceKey key, Variant<ResourceKey, Item> json)
        => json.value;
}
