using Foxel.Common.Collision;
using Foxel.Common.Util;
using Foxel.Common.World.Content.Blocks;
using Foxel.Common.World.Content.Components;
using Foxel.Common.World.Content.Items.Components;
using Foxel.Core.Util;

namespace Foxel.Common.World.Content.Items;

public class BlockItem(ResourceKey block, ImmutableComponentHolder<ItemComponent> components) : Item(components) {
    public readonly ContentReference<Block> Block = new(ContentStores.Blocks, block);

    public override void UseOnBlock(ref ItemStack stack, VoxelWorld world, BlockRaycastHit hit) {
        world.SetBlockState(hit.blockPos + hit.normal.WorldToBlockPosition(), Block.Get().DefaultState);
    }
}
