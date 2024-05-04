using GlmSharp;
using Voxel.Common.Collision;
using Voxel.Common.World.Ecs;
using Voxel.Core.Util;

namespace Voxel.Common.World.Items;

public interface ItemSystem : EcsSystem<ItemSystem, Item, ItemInstance, ItemBuilder>;

public class Item(ItemBuilder builder) : ComponentEntity<Item, ItemInstance, ItemBuilder>(builder) {
    public static readonly ResourceKey UseOnBlockSignal = new("item/use_on_block");

    public override ItemInstance NewInstance()
        => new(this);
}

public class ItemBuilder : ComponentEntityBuilder<Item, ItemInstance, ItemBuilder>;
public class ItemInstance(Item item) : ComponentEntityInstance<Item, ItemInstance, ItemBuilder>(item) {
    public void UseOnBlock(VoxelWorld world, BlockRaycastHit hit)
        => Invoke(Item.UseOnBlockSignal, this, world, hit);
}
