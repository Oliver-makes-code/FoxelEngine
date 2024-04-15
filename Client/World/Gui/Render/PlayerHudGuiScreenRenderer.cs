using GlmSharp;
using Voxel.Client.Gui;
using Voxel.Client.Gui.Canvas;

namespace Voxel.Client.World.Gui.Render;

public class PlayerHudGuiScreenRenderer : ClientGuiScreenRenderer<PlayerHudScreen> {
    public PlayerHudGuiScreenRenderer(PlayerHudScreen screen) : base(screen) {}

    public override void Build() {
        var layer = new GuiCanvas.Layer();
        
        var healthbar = layer.root.AddChild(new(new(1, 1), new(1f, 1f), GuiRect.FromPixelAspectRatioAndHeight(79, 8, 0.08f)));

        for (int i = 0; i < 8; i++)
            healthbar.AddChild(new(new(-1, 1), new(65/64f*(i/4f)-1, 1), GuiRect.FromPixelAspectRatioAndHeight(9, 8, 1), "heart"));
        
        GuiCanvas.PushLayer(layer);
    }
}
