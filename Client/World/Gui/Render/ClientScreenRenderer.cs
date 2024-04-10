using Voxel.Client.Gui;

namespace Voxel.Client.World.Gui.Render;

public abstract class ClientScreenRenderer<T> : GuiScreenRenderer<T> where T : ClientScreen {
    protected ClientScreenRenderer(T screen) : base(screen) {}
}
