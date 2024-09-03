namespace Foxel.Common.World.Content.Items;

public class Inventory {
    private readonly ItemStack[] Items;

    public ItemStack this[int idx] {
        get => Items[idx];
        set => Items[idx] = value;
    }

    public Inventory(int slots) {
        Items = new ItemStack[slots];

        for (int i = 0; i < slots; i++)
            Items[i] = ItemStore.Items.Empty.Get().NewStack();
    }
}
