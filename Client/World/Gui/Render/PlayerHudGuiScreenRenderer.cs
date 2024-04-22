using Voxel.Client.Rendering.Gui;

namespace Voxel.Client.World.Gui.Render;

public class PlayerHudGuiScreenRenderer : ClientGuiScreenRenderer<PlayerHudScreen> {
    public PlayerHudGuiScreenRenderer(PlayerHudScreen screen) : base(screen) {}

    public override void Build(GuiBuilder builder) {
        builder.AddLayer(new("health_bar"), layer => {
            var baseSprite = layer
                .Sprite(new("gui/heart"))
                .WithPosition(new(-2, -2))
                .WithScreenAnchor(new(1, 1))
                .WithTextureAnchor(new(1, 1));

            for (int i = 0; i < 8; i++) {
                layer.AddVertex(baseSprite);
                baseSprite.position.x -= 10;
            }
        });
    }
}
