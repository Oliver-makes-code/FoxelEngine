using System;
using Voxel.Client.Rendering.Gui;

namespace Voxel.Client.World.Gui.Render;

public class PlayerHudGuiScreenRenderer : ClientGuiScreenRenderer<PlayerHudScreen> {
    public PlayerHudGuiScreenRenderer(PlayerHudScreen screen) : base(screen) {}

    public override void Build(GuiBuilder builder) {
        builder.AddLayer(new("hotbar"), layer => {
            layer.AddVertex(layer
                .Sprite(new("gui/hotbar_bg"))
                .WithScreenAnchor(new(0, -1))
                .WithTextureAnchor(new(0, -1))
                .WithPosition(new(0, 4))
            );

            layer.AddVertex(layer
                .Sprite(new("gui/hotbar_select"))
                .WithScreenAnchor(new(0, -1))
                .WithTextureAnchor(new(0, -1))
                .WithPosition(new((18*Screen.Player.selectedHotbarSlot) - 81, 4))
            );
        });
    }
}
