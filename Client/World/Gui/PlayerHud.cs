using GlmSharp;
using Voxel.Common.World.Gui;

namespace Voxel.Client.World.Gui;

public class PlayerHudScreen : ClientGuiScreen {
    public PlayerHudScreen() : base(0) { }
    public override void Click(uint slotIdx, Interaction interaction) => throw new System.NotImplementedException();
}
