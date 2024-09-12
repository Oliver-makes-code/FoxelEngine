using Foxel.Common.Collision;
using Foxel.Common.World.Content.Components;
using Foxel.Common.World.Content.Items.Components;

namespace Foxel.Common.World.Content.Items;

public class Item(ImmutableComponentHolder<ItemComponent> components) {
    public readonly ImmutableComponentHolder<ItemComponent> Components = components;

    public ItemStack NewStack()
        => new(this);

    public virtual void UseOnBlock(ref ItemStack stack, VoxelWorld world, BlockRaycastHit hit) {}
}

public struct ItemStack(Item item) {
    public readonly Item Item = item;
    public readonly MutableComponentHolder<ItemComponent> Components = new(item.Components);
    public uint count;

    public void UseOnBlock(VoxelWorld world, BlockRaycastHit hit) {
        Item.UseOnBlock(ref this, world, hit);
    }
}
