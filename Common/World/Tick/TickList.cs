using Voxel.Core.Util;

namespace Voxel.Common.World.Tick;

public class TickList : DefferedList<Tickable>, Tickable {
    public void Tick() {
        UpdateCollection();
    }
}
