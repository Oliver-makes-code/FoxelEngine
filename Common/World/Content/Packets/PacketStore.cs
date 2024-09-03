using Foxel.Common.Network.Packets;
using Foxel.Common.Network.Packets.C2S.Gameplay;
using Foxel.Common.Network.Packets.C2S.Gameplay.Actions;
using Foxel.Common.Network.Packets.C2S.Handshake;
using Foxel.Common.Network.Packets.S2C.Gameplay;
using Foxel.Common.Network.Packets.S2C.Gameplay.Entity;
using Foxel.Common.Network.Packets.S2C.Gameplay.Tile;
using Foxel.Common.Network.Packets.S2C.Handshake;
using Foxel.Core.Util;
using Greenhouse.Libs.Serialization;

namespace Foxel.Common.World.Content.Packets;

public static class PacketStore {
    public static class C2S {
        public static class Gameplay {
            public static class Actions {
                public static readonly Codec<Packet> BreakBlock = BreakBlockC2SPacket.ProxyCodec;
                public static readonly Codec<Packet> PlaceBlock = PlaceBlockC2SPacket.ProxyCodec;
                public static readonly Codec<Packet> PlayerUseAction = PlayerUseActionC2SPacket.ProxyCodec;
            }
            public static readonly Codec<Packet> PlayerUpdated = PlayerUpdatedC2SPacket.ProxyCodec;
        }
        public static class Handshake {
            public static readonly Codec<Packet> HandshakeDone = HandshakeDoneC2SPacket.ProxyCodec;
        }
    }

    public static class S2C {
        public static class Gameplay {
            public static class Entity {
                public static readonly Codec<Packet> EntityTransformUpdate = EntityTransformUpdateS2CPacket.ProxyCodec;
                public static readonly Codec<Packet> SpawnEntity = SpawnEntityS2CPacket.ProxyCodec;
            }

            public static class Tile {
                public static readonly Codec<Packet> BlockChanged = BlockChangedS2CPacket.ProxyCodec;
            }

            public static readonly Codec<Packet> ChunkData = ChunkDataS2CPacket.ProxyCodec;
            public static readonly Codec<Packet> ChunkUnload = ChunkUnloadS2CPacket.ProxyCodec;
        }

        public static class Handshake {
            public static readonly Codec<Packet> HandshakeDone = HandshakeDoneS2CPacket.ProxyCodec;
            public static readonly Codec<Packet> SetupWorld = SetupWorldS2CPacket.ProxyCodec;
        }
    }

    private static void RegisterPacket(ResourceKey key, Codec<Packet> packet) {
        ContentStores.PacketCodecs.Register(key, packet);
    }

    internal static void RegisterStaticContent() {
        RegisterPacket(new("c2s/gameplay/actions/break_block"), C2S.Gameplay.Actions.BreakBlock);
        RegisterPacket(new("c2s/gameplay/actions/place_block"), C2S.Gameplay.Actions.PlaceBlock);
        RegisterPacket(new("c2s/gameplay/actions/player_use_action"), C2S.Gameplay.Actions.PlayerUseAction);
        RegisterPacket(new("c2s/gameplay/player_updated"), C2S.Gameplay.PlayerUpdated);
        RegisterPacket(new("c2s/handshake/handshake_done"), C2S.Handshake.HandshakeDone);

        RegisterPacket(new("s2c/gameplay/entity/entity_transform_update"), S2C.Gameplay.Entity.EntityTransformUpdate);
        RegisterPacket(new("s2c/gameplay/entity/spawn_entity"), S2C.Gameplay.Entity.SpawnEntity);
        RegisterPacket(new("s2c/gameplay/tile/block_changed"), S2C.Gameplay.Tile.BlockChanged);
        RegisterPacket(new("s2c/gameplay/chunk_data"), S2C.Gameplay.ChunkData);
        RegisterPacket(new("s2c/gameplay/chunk_unload"), S2C.Gameplay.ChunkUnload);
        RegisterPacket(new("s2c/handshake/handshake_done"), S2C.Handshake.HandshakeDone);
        RegisterPacket(new("s2c/handshake/setup_world"), S2C.Handshake.SetupWorld);
    }
}
