using Common.Network.Packets.S2C.Handshake;
using Common.Server.Components.Networking;
using GlmSharp;
using Voxel.Common.Config;
using Voxel.Common.Entity;

namespace Common.Server.Components;

public class PlayerManager : ServerComponent {

    public readonly Dictionary<ServerConnectionContext, PlayerEntity> ContextToPlayer = new();
    public readonly Dictionary<PlayerEntity, ServerConnectionContext> PlayerToContext = new();

    public PlayerManager(VoxelServer server) : base(server) {

    }

    public override void OnServerStart() {
        Server.ConnectionManager.OnConnectionMade += OnConnectionMade;
    }
    public override void Tick() {

    }
    public override void OnServerStop() {
        Server.ConnectionManager.OnConnectionMade -= OnConnectionMade;
    }


    private void OnConnectionMade(ServerConnectionContext context) {
        context.GameplayStart += () => {
            Console.WriteLine("Server:Creating player entity...");

            PlayerEntity pEntity = new PlayerEntity();
            pEntity.ID = Guid.NewGuid();

            ContextToPlayer[context] = pEntity;
            PlayerToContext[pEntity] = context;

            context.SetPlayerEntity(pEntity);

            Server.WorldManager.DefaultWorld.AddEntity(pEntity, dvec3.Zero, 0);

            Console.WriteLine("Server:Sending player to world...");
            context.SendPacket(new SetupWorld());
            context.SetupViewArea(3);
        };
    }
}
