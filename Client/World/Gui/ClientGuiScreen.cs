using Voxel.Client.Gui;
using Voxel.Common.World.Gui;

namespace Voxel.Client.World.Gui;

public abstract class ClientGuiScreen : GuiScreen {
    protected ClientGuiScreen(uint numSlots) : base(numSlots) { }

    public sealed override void Open() {
        var renderer = GuiScreenRendererRegistry.GetRenderer(this);
        renderer.Build();
        
    }
    
    public sealed override void Close() {
        
    }
}
