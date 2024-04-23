using GlmSharp;
using Voxel.Common.Network.Packets.S2C;
using Voxel.Common.Network.Packets.S2C.Gameplay.Entity;
using Voxel.Common.Network.Packets.S2C.Handshake;
using Voxel.Common.Network.Packets.Utils;
using Voxel.Common.Server.Components.Networking;
using Voxel.Common.Util;
using Voxel.Common.World.Entity;
using Voxel.Common.World.Entity.Player;

namespace Voxel.Common.Server.Components;

public class PlayerManager : ServerComponent {

    public readonly Dictionary<ServerConnectionContext, PlayerEntity> ContextToPlayer = new();
    public readonly Dictionary<PlayerEntity, ServerConnectionContext> PlayerToContext = new();

    public PlayerManager(VoxelServer server) : base(server) {

    }

    public override void OnServerStart() {
        Server.ConnectionManager.OnConnectionMade += OnConnectionMade;
    }

    public override void Tick() {}

    public override void OnServerStop() {
        Server.ConnectionManager.OnConnectionMade -= OnConnectionMade;
    }

    //Sends a packet to all players that can see a given position.
    public void SendViewPacket(S2CPacket packet, dvec3 position) {
        foreach (var key in ContextToPlayer.Keys)
            key.SendPositionedPacket(position, packet, false);
        PacketPool.Return(packet);
    }


    //Sends a packet to all players.
    public void SendPacket(S2CPacket packet) {
        foreach (var key in ContextToPlayer.Keys)
            key.SendPacket(packet, false);
        PacketPool.Return(packet);
    }

    private void OnConnectionMade(ServerConnectionContext context) {
        context.GameplayStart += () => {
            VoxelServer.Logger.Info("Creating player entity...");

            PlayerEntity pEntity = new PlayerEntity();
            pEntity.id = context.playerID;

            ContextToPlayer[context] = pEntity;
            PlayerToContext[pEntity] = context;

            context.SetPlayerEntity(pEntity);

            VoxelServer.Logger.Info("Sending player to world...");
            context.SendPacket(PacketPool.GetPacket<SetupWorldS2CPacket>());
            context.SetupViewArea(Server.WorldManager.DefaultWorld, new dvec3(16, 16, 16).WorldToChunkPosition(), 5);

            Server.WorldManager.DefaultWorld.AddEntity(pEntity, new(16, 16, 16), dvec2.Zero);
        };
    }
}
