using GlmSharp;
using Foxel.Common.Collision;
using Foxel.Common.Server;
using Foxel.Common.World.Items;

namespace Foxel.Common.World.Entity.Player;

public class PlayerEntity : LivingEntity {
    public readonly Inventory Inventory = new(10);

    public override float eyeHeight { get; } = 1.62f;
    public override Box boundingBox { get; } = Box.FromPosSize(new(0, 0, 0), new dvec3(1, 2, 1) * 0.95);

    public int selectedHotbarSlot { get; private set; }

    public ItemInstance selectedItem => Inventory[selectedHotbarSlot];

    public PlayerEntity() {
        Inventory[0] = VoxelServer.ItemContentManager[new("stone_block")].NewInstance();
        Inventory[1] = VoxelServer.ItemContentManager[new("dirt_block")].NewInstance();
        Inventory[2] = VoxelServer.ItemContentManager[new("grass_block")].NewInstance();
        Inventory[5] = VoxelServer.ItemContentManager[new("cobblestone_block")].NewInstance();
    }

    public void SetSelectedSlot(int slot) {
        selectedHotbarSlot = ((slot % 10) + 10) % 10;

        MarkDirty();
    }
}
