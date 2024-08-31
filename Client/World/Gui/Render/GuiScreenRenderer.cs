using Foxel.Client.Rendering.Gui;
using Foxel.Common.World.Gui;

namespace Foxel.Client.World.Gui.Render;

public abstract class GuiScreenRenderer<T> : GuiScreenRenderer where T : GuiScreen {
    public readonly T Screen;

    protected GuiScreenRenderer(T screen) {
        Screen = screen;
    }

    public override GuiScreen GetScreen()
        => Screen;
}

public abstract class GuiScreenRenderer {
    public abstract GuiScreen GetScreen();

    public abstract void Build(GuiBuilder builder);
}
