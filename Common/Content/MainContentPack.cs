using Foxel.Common.Network.Packets.C2S.Gameplay;
using Foxel.Common.Network.Packets.C2S.Gameplay.Actions;
using Foxel.Common.Network.Packets.C2S.Handshake;
using Foxel.Common.Network.Packets.S2C.Gameplay;
using Foxel.Common.Network.Packets.S2C.Gameplay.Entity;
using Foxel.Common.Network.Packets.S2C.Gameplay.Tile;
using Foxel.Common.Network.Packets.S2C.Handshake;
using Foxel.Common.Tile;
using Foxel.Core.Util;
using Foxel.Common.World.Entity.Player;

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
        LoadPacketTypes();
        LoadEntityTypes();
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

    private void LoadPacketTypes() {

        //C2S
        AddPacketType<PlayerUpdatedC2SPacket>(new("c2s_player_update"));
        AddPacketType<HandshakeDoneC2SPacket>(new("c2s_handshake_done"));

        AddPacketType<PlaceBlockC2SPacket>(new("c2s_place_block"));
        AddPacketType<BreakBlockC2SPacket>(new("c2s_break_block"));
        AddPacketType<PlayerUseActionC2SPacket>(new("c2s_player_use"));

        //S2C
        AddPacketType<HandshakeDoneS2CPacket>(new("s2c_handshake_done"));
        AddPacketType<SetupWorldS2CPacket>(new("s2c_setup_world"));

        AddPacketType<ChunkDataS2CPacket>(new("s2c_chunk_data"));
        AddPacketType<ChunkUnloadS2CPacket>(new("s2c_chunk_unload"));

        AddPacketType<EntityTransformUpdateS2CPacket>(new("s2c_entity_transform"));
        AddPacketType<SpawnEntityS2CPacket>(new("s2c_entity_spawn"));

        AddPacketType<BlockChangedS2CPacket>(new("s2c_block_changed"));
    }

    private void LoadEntityTypes() {
        AddEntityType<PlayerEntity>(new("player"));
    }
}
