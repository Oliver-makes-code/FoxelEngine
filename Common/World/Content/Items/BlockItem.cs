using Foxel.Common.Collision;
using Foxel.Common.Content;
using Foxel.Common.Util;
using Foxel.Common.World.Content.Components;
using Foxel.Common.World.Content.Items.Components;
using Foxel.Core.Util;

namespace Foxel.Common.World.Content.Items;

public class BlockItem(ResourceKey block, ImmutableComponentHolder<ItemComponent> components) : Item(components) {
    public readonly ResourceKey Block = block;

    public override void UseOnBlock(ref ItemStack stack, VoxelWorld world, BlockRaycastHit hit) {
        if (ContentDatabase.Instance.Registries.Blocks.IdToEntry(Block, out var block))
            world.SetBlock(hit.blockPos + hit.normal.WorldToBlockPosition(), block);
    }
}
