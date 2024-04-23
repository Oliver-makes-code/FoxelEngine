using GlmSharp;
using Voxel.Common.Collision;

namespace Voxel.Common.World.Entity.Player;

public class PlayerEntity : LivingEntity {
    public override float eyeHeight { get; } = 1.62f;
    public override Box boundingBox { get; } = Box.FromPosSize(new(0, 0, 0), new dvec3(1, 2, 1) * 0.95);

    public int selectedHotbarSlot { get; private set; }

    public void SetSelectedSlot(int slot) {
        selectedHotbarSlot = ((slot % 10) + 10) % 10;

        MarkDirty();
    }
}
