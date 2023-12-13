using Voxel.Common.Network.Packets.C2S.Gameplay;
using Voxel.Common.Network.Packets.C2S.Handshake;
using Voxel.Common.Network.Packets.S2C.Gameplay;
using Voxel.Common.Network.Packets.S2C.Gameplay.Entity;
using Voxel.Common.Network.Packets.S2C.Handshake;
using Voxel.Common.Tile;
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
        Air = AddBlock(new Block("air", new BlockSettings.Builder {
            IsAir = true
        }));
        Stone = AddBlock(new Block("stone"));
        Dirt = AddBlock(new Block("dirt"));
        Grass = AddBlock(new Block("grass"));
    }

    private void LoadPacketTypes() {

        //C2S
        AddPacketType<PlayerUpdated>("c2s_player_update");
        AddPacketType<C2SHandshakeDone>("c2s_handshake_done");

        //S2C
        AddPacketType<S2CHandshakeDone>("s2c_handshake_done");
        AddPacketType<SetupWorld>("s2c_setup_world");

        AddPacketType<ChunkData>("s2c_chunk_data");
        AddPacketType<ChunkUnload>("s2c_chunk_unload");

        AddPacketType<EntityTransformUpdate>("s2c_entity_transform");
        AddPacketType<SpawnEntity>("s2c_entity_spawn");

    }

    private void LoadEntityTypes() {
        AddEntityType<PlayerEntity>("player");
    }
}
