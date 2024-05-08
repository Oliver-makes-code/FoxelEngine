using System;
using Voxel.Client.Rendering.Gui;
using Voxel.Common.Server;
using Voxel.Core.Util;

namespace Voxel.Client.World.Gui.Render;

public class PlayerHudGuiScreenRenderer : ClientGuiScreenRenderer<PlayerHudScreen> {
    public PlayerHudGuiScreenRenderer(PlayerHudScreen screen) : base(screen) {}

    public override void Build(GuiBuilder builder) {
        var player = Screen.Player;

        builder.AddLayer(new("crosshair"), layer => {
            layer.AddVertex(layer
                .Sprite(new("gui/crosshair"))
            );
        });

        builder.AddLayer(new("hotbar_bg"), layer => {
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
                .WithPosition(new((18*player.selectedHotbarSlot) - 81, 4))
            );
        });

        builder.AddLayer(new("hotbar_fg"), layer => {
            for (int i = 0; i < 10; i++) {
                var item = player.Inventory[i];
                if (item.Entity.ModelKey == null)
                    continue;
                
                layer.AddVertex(layer
                    .Item(item)
                    .WithScreenAnchor(new(0, -1))
                    .WithTextureAnchor(new(0, 0))
                    .WithPosition(new((18*i) - 81, 14))
                );
            }
        });
    }
}
