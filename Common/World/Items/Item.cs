using GlmSharp;
using Voxel.Common.Collision;
using Voxel.Common.World.Ecs;
using Voxel.Core.Util;

namespace Voxel.Common.World.Items;

public interface ItemSystem : EcsSystem<ItemSystem, Item, ItemInstance, ItemBuilder>;

public class Item : ComponentEntity<Item, ItemInstance, ItemBuilder> {
    public delegate void UseOnBlockCallback(ItemInstance instance, VoxelWorld world, BlockRaycastHit hit);

    public readonly ResourceKey? ModelKey;

    public Item(ItemBuilder builder, ResourceKey? modelKey) : base(builder) {
        ModelKey = modelKey;
    }

    public override ItemInstance NewInstance()
        => new(this);
}

public class ItemBuilder : ComponentEntityBuilder<Item, ItemInstance, ItemBuilder>;
public class ItemInstance(Item item) : ComponentEntityInstance<Item, ItemInstance, ItemBuilder>(item) {
    public void UseOnBlock(VoxelWorld world, BlockRaycastHit hit)
        => Invoke<Item.UseOnBlockCallback>(c => c(this, world, hit));
}
