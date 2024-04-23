using GlmSharp;
using Voxel.Client.World.Entity;
using Voxel.Common.World.Entity.Player;
using Voxel.Common.World.Gui;

namespace Voxel.Client.World.Gui;

public class PlayerHudScreen : ClientGuiScreen {
    public readonly ControlledClientPlayerEntity Player;
    public PlayerHudScreen(ControlledClientPlayerEntity player) : base(0) {
        Player = player;
    }
    public override void Click(uint slotIdx, Interaction interaction) => throw new System.NotImplementedException();
}
