namespace Foxel.Common.World.Gui.Slot;

public abstract class GuiSlot {
    public virtual void Click(GuiScreen screen, uint slotIdx, Interaction interaction) {}
}
