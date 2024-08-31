using Foxel.Common.World.Gui.Slot;

namespace Foxel.Common.World.Gui; 

public abstract class GuiScreen {
    
    private readonly GuiSlot[] Slots;
    public bool dirty { get; private set; } = false;

    protected GuiScreen(uint numSlots) {
        Slots = new GuiSlot[numSlots];
    }
    
    /// <summary>
    /// Performs server-side logic when slots are interacted with
    /// </summary>
    public virtual void Click(uint slotIdx, Interaction interaction)
        => GetSlot<GuiSlot>(slotIdx)?.Click(this, slotIdx, interaction);

    public void SetSlot(uint idx, GuiSlot slot) 
        => Slots[idx] = slot;
    
    public T? GetSlot<T>(uint idx) where T : GuiSlot
        => Slots[idx] as T;

    public void MarkDirty() {
        dirty = true;
    }

    public virtual void Open() {}
    
    public virtual void Close() {}

    public virtual void Tick() {}
}

public enum Interaction {
    Primary,
    Secondary,
    Tertiary
}
