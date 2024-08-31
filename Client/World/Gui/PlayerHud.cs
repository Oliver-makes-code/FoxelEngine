using GlmSharp;
using Foxel.Client.World.Entity;
using Foxel.Common.World.Entity.Player;
using Foxel.Common.World.Gui;

namespace Foxel.Client.World.Gui;

public class PlayerHudScreen : ClientGuiScreen {
    public readonly ControlledClientPlayerEntity Player;
    public PlayerHudScreen(ControlledClientPlayerEntity player) : base(0) {
        Player = player;
    }
    public override void Click(uint slotIdx, Interaction interaction) => throw new System.NotImplementedException();
}
