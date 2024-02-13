using GlmSharp;
using Voxel.Client.Gui;

namespace Voxel.Client.World.Gui;

public class PlayerHudScreen : ClientGuiScreen {
    public PlayerHudScreen() : base(0) { }
    public override void Click(uint slotIdx, Interaction interaction) => throw new System.NotImplementedException();
}

public class PlayerHudRenderer : GuiScreenRenderer<PlayerHudScreen> {
    public PlayerHudRenderer(PlayerHudScreen screen) : base(screen) {}

    public override void Build() {
        GuiCanvas.Layer l = new();
        
        var healthbar = l.root.AddChild(new(new(1, 1), new(1, 1), new vec2(0.8f, 0.1f)));
        
        for (int i = 0; i < 7; i++)
            healthbar.AddChild(new(new(1, 0), new(1 - 0.11f * i, 0), GuiRect.FromPixelAspectRatioAndHeight(9, 8, 1), "heart"));
        
        GuiCanvas.PushLayer(l);
    }

}
