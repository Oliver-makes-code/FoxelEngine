﻿namespace Voxel.Common.World.Gui; 

public class GuiSlot {}

public abstract class GuiScreen {
    
    private readonly GuiSlot[] Slots;

    protected GuiScreen(uint numSlots) {
        Slots = new GuiSlot[numSlots];
    }
    
    /// <summary>
    /// Performs server-side logic when slots are interacted with
    /// </summary>
    public abstract void Click(uint slotIdx, Interaction interaction);

    public void SetSlot(uint idx, GuiSlot slot) {
        Slots[idx] = slot;
    }
    
    public T? GetSlot<T>(uint idx) where T : GuiSlot {
        return Slots[idx] as T;
    }

    public virtual void Open() {
        
    }
    
    public virtual void Close() {
        
    }
    
    public enum Interaction {
        Primary,
        Secondary,
        Tertiary
    }
}