using GlmSharp;
using Foxel.Common.Util;
using Foxel.Common.World;
using Foxel.Common.World.Storage;

namespace Foxel.Common.Server.World;

public class ServerChunk : Chunk {
    private readonly ServerWorld ServerWorld;
    private readonly HashSet<ivec3> Positions = [];

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
    }
}
