namespace Foxel.Client.World.Gui.Render;

public abstract class ClientGuiScreenRenderer<T> : GuiScreenRenderer<T> where T : ClientGuiScreen {
    protected ClientGuiScreenRenderer(T screen) : base(screen) {}
}
