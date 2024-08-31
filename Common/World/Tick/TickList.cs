using Foxel.Core.Util;

namespace Foxel.Common.World.Tick;

public class TickList : DefferedList<Tickable>, Tickable {
    public void Tick() {
        UpdateCollection();
    }
}
