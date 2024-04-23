using System;
using Voxel.Client.Rendering.Gui;
using Voxel.Core.Util;

namespace Voxel.Client.World.Gui.Render;

public class PlayerHudGuiScreenRenderer : ClientGuiScreenRenderer<PlayerHudScreen> {
    public PlayerHudGuiScreenRenderer(PlayerHudScreen screen) : base(screen) {}

    public override void Build(GuiBuilder builder) {
        builder.AddLayer(new("crosshair"), layer => {
            layer.AddVertex(layer
                .Sprite(new("gui/crosshair"))
            );
        });

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

            for (int i = 0; i < 10; i++) {
                if (i > 3)
                    continue;
                ResourceKey id = i switch {
                    0 => new("gui/stone_item"),
                    1 => new("gui/dirt_item"),
                    2 => new("gui/grass_item"),
                    _ => new("gui/cobblestone_item")
                };
                layer.AddVertex(layer
                    .Sprite(id)
                    .WithSize(new(16, 16))
                    .WithScreenAnchor(new(0, -1))
                    .WithTextureAnchor(new(0, -1))
                    .WithPosition(new((18*i) - 81, 6))
                );
            }
        });
    }
}
