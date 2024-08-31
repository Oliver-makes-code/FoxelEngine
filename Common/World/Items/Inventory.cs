using Foxel.Common.Server;

namespace Foxel.Common.World.Items;

public class Inventory {
    private readonly ItemInstance[] Items;

    public ItemInstance this[int idx] {
        get => Items[idx];
        set => Items[idx] = value;
    }

    public Inventory(int slots) {
        Items = new ItemInstance[slots];

        for (int i = 0; i < slots; i++)
            Items[i] = VoxelServer.ItemContentManager[new("empty")].NewInstance();
    }
}
