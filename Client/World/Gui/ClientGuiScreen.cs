using Foxel.Common.World.Gui;

namespace Foxel.Client.World.Gui;

public abstract class ClientGuiScreen : GuiScreen {
    protected ClientGuiScreen(uint numSlots) : base(numSlots) { }
}
