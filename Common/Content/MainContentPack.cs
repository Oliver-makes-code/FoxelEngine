using Voxel.Common.Network.Packets.C2S.Gameplay;
using Voxel.Common.Network.Packets.C2S.Gameplay.Actions;
using Voxel.Common.Network.Packets.C2S.Handshake;
using Voxel.Common.Network.Packets.S2C.Gameplay;
using Voxel.Common.Network.Packets.S2C.Gameplay.Entity;
using Voxel.Common.Network.Packets.S2C.Gameplay.Tile;
using Voxel.Common.Network.Packets.S2C.Handshake;
using Voxel.Common.Tile;
using Voxel.Core.Util;
using Voxel.Common.World.Entity.Player;

namespace Voxel.Common.Content;

public class MainContentPack : ContentPack {

    public static readonly MainContentPack Instance = new();

    public Block Air;
    public Block Stone;
    public Block Dirt;
    public Block Grass;

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
