using Foxel.Common.Network.Packets.C2S.Gameplay;
using Foxel.Common.Network.Packets.C2S.Gameplay.Actions;
using Foxel.Common.Network.Packets.C2S.Handshake;
using Foxel.Common.Network.Packets.S2C.Gameplay;
using Foxel.Common.Network.Packets.S2C.Gameplay.Entity;
using Foxel.Common.Network.Packets.S2C.Gameplay.Tile;
using Foxel.Common.Network.Packets.S2C.Handshake;
using Foxel.Common.Tile;
using Foxel.Core.Util;
using Foxel.Common.World.Content.Entities.Player;

namespace Foxel.Common.Content;

public class MainContentPack : ContentPack {

    public static readonly MainContentPack Instance = new();

    public Block Air;
    public Block Stone;
    public Block Dirt;
    public Block Grass;
    public Block cobblestone;

    private MainContentPack() : base("main") {}

    public override void Load() {
        base.Load();

        LoadBlocks();
    }

    private void LoadBlocks() {

        Air = AddBlock(new Block(new("air"), new BlockSettings.Builder {
            isAir = true
        }));
        Stone = AddBlock(new Block(new("stone")));
        cobblestone = AddBlock(new Block(new("cobblestone")));
        Dirt = AddBlock(new Block(new("dirt")));
        Grass = AddBlock(new GrassBlock(new("grass"), new BlockSettings.Builder {
            ticksRandomly = true
        }));

    }
}
