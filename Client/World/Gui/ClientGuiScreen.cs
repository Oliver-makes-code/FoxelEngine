using Voxel.Common.World.Gui;

namespace Voxel.Client.World.Gui;

public abstract class ClientGuiScreen : GuiScreen {
    protected ClientGuiScreen(uint numSlots) : base(numSlots) { }
}
