using Newtonsoft.Json.Linq;
using Voxel.Common.Collision;
using Voxel.Common.Content;
using Voxel.Common.Server;
using Voxel.Common.Tile;
using Voxel.Common.Util;
using Voxel.Core.Util;

namespace Voxel.Common.World.Items.System;

public record BlockItemSystem(Block block) : ItemSystem {
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

    public void Register(ItemBuilder builder) {
        builder.Listen<Item.UseOnBlockCallback>(UseOnBlock);
    }

    public void UseOnBlock(ItemInstance instance, VoxelWorld world, BlockRaycastHit hit) {
        world.SetBlock(hit.blockPos + hit.normal.WorldToBlockPosition(), block);
    }
}
