using System;
using Foxel.Client.Rendering.Gui;
using Foxel.Common.Server;
using Foxel.Common.World.Content.Items;
using Foxel.Core.Util;

namespace Foxel.Client.World.Gui.Render;

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
                var stack = player.Inventory[i];
                if (stack.Item == ItemStore.Items.Empty.Get())
                    continue;
                
                layer.AddVertex(layer
                    .Item(stack)
                    .WithScreenAnchor(new(0, -1))
                    .WithTextureAnchor(new(0, 0))
                    .WithPosition(new((18*i) - 81, 14))
                );
            }
        });
    }
}
