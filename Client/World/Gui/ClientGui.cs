using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using Voxel.Client.Gui;
using Voxel.Common.World.Gui;

namespace Voxel.Client.World.Gui;

public abstract class ClientGuiScreen : GuiScreen {
    protected ClientGuiScreen(uint numSlots) : base(numSlots) { }

    public sealed override void Open() {
        var r = GuiScreenRendererRegistry.GetRenderer(this);
    }
    public sealed override void Close() {
        
    }
}

public abstract class ClientGuiScreenRenderer<T> : GuiScreenRenderer<T> where T : ClientGuiScreen {
    protected ClientGuiScreenRenderer(T screen) : base(screen) {}
}
