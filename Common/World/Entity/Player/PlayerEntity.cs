using GlmSharp;
using Voxel.Common.Collision;
using Voxel.Common.Server;
using Voxel.Common.World.Items;

namespace Voxel.Common.World.Entity.Player;

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
        Inventory[3] = VoxelServer.ItemContentManager[new("cobblestone_block")].NewInstance();
    }

    public void SetSelectedSlot(int slot) {
        selectedHotbarSlot = ((slot % 10) + 10) % 10;

        MarkDirty();
    }
}
