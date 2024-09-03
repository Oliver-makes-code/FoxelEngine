using GlmSharp;
using Foxel.Common.Collision;
using Foxel.Common.World.Content.Items;

namespace Foxel.Common.World.Content.Entities.Player;

public class PlayerEntity : LivingEntity {
    public readonly Inventory Inventory = new(10);

    public override float eyeHeight { get; } = 1.62f;
    public override Box boundingBox { get; } = Box.FromPosSize(new(0, 0, 0), new dvec3(1, 2, 1) * 0.95);

    public int selectedHotbarSlot { get; private set; }

    public ItemStack selectedItem => Inventory[selectedHotbarSlot];

    public PlayerEntity() {
        Inventory[0] = ItemStore.Items.StoneBlock.Get().NewStack();
        Inventory[1] = ItemStore.Items.DirtBlock.Get().NewStack();
        Inventory[2] = ItemStore.Items.GrassBlock.Get().NewStack();
        Inventory[5] = ItemStore.Items.CobblestoneBlock.Get().NewStack();
    }

    public void SetSelectedSlot(int slot) {
        selectedHotbarSlot = ((slot % 10) + 10) % 10;

        MarkDirty();
    }
}
