using Foxel.Common.World.Content.Blocks;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content;

public class BlockContentManager() : ServerContentManager<Variant<ResourceKey, Block>, Block>(Codec, ContentStores.Blocks) {
    public static readonly Codec<Variant<ResourceKey, Block>> Codec = new RecordVariantCodec<ResourceKey, Block>(
        ResourceKey.Codec,
        ContentStores.BlockCodecs.GetValue
    );

    public override string ContentDir()
        => "blocks";

    public override Block Load(ResourceKey key, Variant<ResourceKey, Block> json)
        => json.value;
}
