using GlmSharp;
using Voxel.Common.Content;
using Voxel.Common.Network.Packets.S2C.Gameplay.Entity;
using Voxel.Common.Network.Packets.S2C.Gameplay.Tile;
using Voxel.Common.Network.Packets.Utils;
using Voxel.Common.Tile;
using Voxel.Common.World;
using Voxel.Common.World.Entity;
using Voxel.Common.World.Generation;

namespace Voxel.Common.Server.World;

public class ServerWorld : VoxelWorld {
    public readonly VoxelServer Server;

    public ServerWorld(VoxelServer server) {
        Server = server;
    }

    protected override Chunk CreateChunk(ivec3 pos) {
        var newChunk = new ServerChunk(pos, this);
        SimpleGenerator.GenerateChunk(newChunk);
        return newChunk;
    }

    public override void AddEntity(Entity entity, dvec3 position, dvec2 rotation) {
        base.AddEntity(entity, position, rotation);

        var spawnEntityPacket = PacketPool.GetPacket<SpawnEntityS2CPacket>();
        spawnEntityPacket.Init(entity);

        Server.PlayerManager.SendViewPacket(spawnEntityPacket, position);
    }

    public override void ProcessEntity(Entity e) {
        base.ProcessEntity(e);

        //TODO - sync packet
    }

    protected override void OnBlockChanged(ivec3 position, Block newBlock) {
        base.OnBlockChanged(position, newBlock);

        if (!ContentDatabase.Instance.Registries.Blocks.EntryToRaw(newBlock, out var rawId))
            return;

        var blockPacket = PacketPool.GetPacket<BlockChangedS2CPacket>();
        blockPacket.Position = position;
        blockPacket.BlockID = rawId;

        Server.PlayerManager.SendViewPacket(blockPacket, position);
    }
}
