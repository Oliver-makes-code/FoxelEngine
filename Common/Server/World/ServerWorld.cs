using GlmSharp;
using Foxel.Common.Network.Packets.S2C.Gameplay.Entity;
using Foxel.Common.Network.Packets.Utils;
using Foxel.Common.World;
using Foxel.Common.World.Generation;
using Foxel.Common.World.Content.Entities;

namespace Foxel.Common.Server.World;

public class ServerWorld : VoxelWorld {
    public readonly VoxelServer Server;

    public ServerWorld(VoxelServer server) {
        Server = server;
    }

    public override void AddEntity(Entity entity, dvec3 position, dvec2 rotation) {
        base.AddEntity(entity, position, rotation);

        var spawnEntityPacket = PacketPool.GetPacket<SpawnEntityS2CPacket>();
        spawnEntityPacket.Init(entity);

        Server.PlayerManager.SendViewPacket(spawnEntityPacket, position);
    }

    public override void ProcessEntity(Entity e) {
        base.ProcessEntity(e);

        //TODO: sync packet
    }

    protected override Chunk CreateChunk(ivec3 pos) {
        var newChunk = new ServerChunk(pos, this);
        SimpleGenerator.GenerateChunk(newChunk);
        return newChunk;
    }
}
