using Voxel.Common.World.Gui;

namespace Voxel.Client.World.Gui.Render;

public abstract class GuiScreenRenderer<T> : GuiScreenRenderer where T : GuiScreen {

    public readonly T Screen;

    protected GuiScreenRenderer(T screen) {
        Screen = screen;
    }
}

public abstract class GuiScreenRenderer {
    public abstract void Build();
}
