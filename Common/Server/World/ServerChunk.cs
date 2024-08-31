using GlmSharp;
using Foxel.Common.Util;
using Foxel.Common.World;
using Foxel.Common.World.Storage;
using Foxel.Common.Tile;
using Foxel.Common.Network.Packets.Utils;
using Foxel.Common.Network.Packets.S2C.Gameplay.Tile;
using Foxel.Common.Content;

namespace Foxel.Common.Server.World;

public class ServerChunk : Chunk {
    private readonly ServerWorld ServerWorld;
    private readonly HashSet<ivec3> Positions = [];
    private readonly HashSet<ivec3> ChangedSet = [];
    private readonly List<ivec3> ChangedList = [];

    public ServerChunk(ivec3 chunkPosition, ServerWorld world, ChunkStorage? storage = null) : base(chunkPosition, world, storage) {
        ServerWorld = world;
    }

    public override void Tick() {
        base.Tick();
        
        // Randomly tick blocks
        Positions.Clear();
        for (int i = 0; i < RandomTickCount; i++) {
            var pos = World.Random.NextChunkPos();
            var block = GetBlock(pos);
            if (Positions.Contains(pos) || !block.TicksRandomly)
                continue;
            Positions.Add(pos);
            block.RandomTick(World, WorldPosition + pos);
        }

        if (ChangedList.Count != 0) {
            var pkt = PacketPool.GetPacket<BlockChangedS2CPacket>();

            pkt.worldPos = WorldPosition;
            pkt.updates = new BlockChangedS2CPacket.Single[ChangedList.Count];

            for (int i = 0; i < ChangedList.Count; i++) {
                var pos = ChangedList[i];
                var block = GetBlock(pos);
                if (!ContentDatabase.Instance.Registries.Blocks.EntryToRaw(block, out var id))
                    return;
                pkt.updates[i] = new() {
                    position = pos,
                    blockId = id
                };
            }

            ServerWorld.Server.PlayerManager.SendViewPacket(pkt, WorldPosition);
        }
        ChangedList.Clear();
        ChangedSet.Clear();
    }

    public override void SetBlock(ivec3 position, Block toSet) {
        base.SetBlock(position, toSet);

        if (!ChangedSet.Contains(position)) {
            ChangedList.Add(position);
            ChangedSet.Add(position);
        }
    }
}
