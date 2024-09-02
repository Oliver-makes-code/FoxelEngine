using Newtonsoft.Json.Linq;
using Foxel.Common.Collision;
using Foxel.Common.Content;
using Foxel.Common.Server;
using Foxel.Common.Tile;
using Foxel.Common.Util;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;
using Foxel.Common.World.Content;

namespace Foxel.Common.World.Items.System;

public record BlockItemSystem(Block block) : ItemSystem {
    public static readonly Codec<ItemSystem> Codec = new ItemContentManager.ItemSystemCodec<Config>(
        Config.Codec,
        Create
    );

    public static BlockItemSystem? Create(JObject? options) {
        if (options == null)
            return null;
        string? key = (string?)options["block"];
        if (key == null)
            return null;
        if (ContentDatabase.Instance.Registries.Blocks.IdToEntry(new ResourceKey(key), out var block))
            return new BlockItemSystem(block);
        return null;
    }

    private static BlockItemSystem Create(Config config) {
        if (ContentDatabase.Instance.Registries.Blocks.IdToEntry(new ResourceKey(config.Block), out var block))
            return new BlockItemSystem(block);
        throw new Exception($"Block {config.Block} not found.");
    }

    public void Register(ItemBuilder builder) {
        builder.Listen<Item.UseOnBlockCallback>(UseOnBlock);
    }

    public void UseOnBlock(ItemInstance instance, VoxelWorld world, BlockRaycastHit hit) {
        world.SetBlock(hit.blockPos + hit.normal.WorldToBlockPosition(), block);
    }

    private record Config(string Block) {
        public static readonly Codec<Config> Codec = RecordCodec<Config>.Create(
            Codecs.String.Field<Config>("block", it => it.Block),
            (block) => new(block)
        );
    }
}
