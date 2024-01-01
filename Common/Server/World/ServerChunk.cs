using GlmSharp;
using Voxel.Common.Util;
using Voxel.Common.World;
using Voxel.Common.World.Storage;

namespace Voxel.Common.Server.World;

public class ServerChunk : Chunk {
    private readonly ServerWorld ServerWorld;

    public ServerChunk(ivec3 chunkPosition, ServerWorld world, ChunkStorage? storage = null) : base(chunkPosition, world, storage) {
        ServerWorld = world;
    }

    public override void Tick() {
        base.Tick();
        
        // Randomly tick blocks
        var positions = new HashSet<ivec3>();
        for (int i = 0; i < RandomTickCount; i++) {
            var pos = World.Random.NextChunkPos();
            var block = GetBlock(pos);
            if (positions.Contains(pos) || !block.TicksRandomly)
                continue;
            positions.Add(pos);
            block.RandomTick(World, WorldPosition + pos);
        }
    }
}
