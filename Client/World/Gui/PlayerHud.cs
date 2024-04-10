using GlmSharp;
using Voxel.Client.Gui;
using Voxel.Client.Gui.Canvas;
using Voxel.Common.World.Gui;

namespace Voxel.Client.World.Gui;

public class PlayerHudScreen : ClientScreen {
    public PlayerHudScreen() : base(0) { }
    public override void Click(uint slotIdx, Interaction interaction) => throw new System.NotImplementedException();
}
