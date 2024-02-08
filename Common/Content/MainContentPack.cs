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
        AddPacketType<PlayerUpdated>(new("c2s_player_update"));
        AddPacketType<C2SHandshakeDone>(new("c2s_handshake_done"));

        AddPacketType<PlaceBlock>(new("c2s_place_block"));

        //S2C
        AddPacketType<S2CHandshakeDone>(new("s2c_handshake_done"));
        AddPacketType<SetupWorld>(new("s2c_setup_world"));

        AddPacketType<ChunkData>(new("s2c_chunk_data"));
        AddPacketType<ChunkUnload>(new("s2c_chunk_unload"));

        AddPacketType<EntityTransformUpdate>(new("s2c_entity_transform"));
        AddPacketType<SpawnEntity>(new("s2c_entity_spawn"));

        AddPacketType<BlockChanged>(new("s2c_block_changed"));
    }

    private void LoadEntityTypes() {
        AddEntityType<PlayerEntity>(new("player"));
    }
}
