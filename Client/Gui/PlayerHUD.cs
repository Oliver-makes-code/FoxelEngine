using GlmSharp;
using Voxel.Common.Gui;

namespace Voxel.Client.Gui;

public class PlayerHUDScreen : ClientGuiScreen {
    public override void RegisterServerInteractions() {}

    public override void BuildClientGui() {
        GuiCanvas.Layer l = new();
        var healthbar = l.root.AddChild(new(new(1, 1), new(1, 1), new vec2(0.8f, 0.1f)));
        
        for (int i = 0; i < 7; i++)
            healthbar.AddChild(new(new(1, 0), new(1 - 0.11f * i, 0), GuiRect.FromPixelAspectRatioAndHeight(9, 8, 1), "heart"));
        
        GuiCanvas.PushLayer(l);
    }

}
