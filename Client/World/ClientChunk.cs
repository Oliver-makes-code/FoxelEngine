using GlmSharp;
using Foxel.Common.World;
using Foxel.Common.World.Storage;

namespace Foxel.Client.World;

public class ClientChunk : Chunk {

    public bool isFilled;

    public ClientChunk(ivec3 chunkPosition, VoxelWorld world, ChunkStorage? storage = null) : base(chunkPosition, world, storage) {
        if (storage != null)
            isFilled = true;
    }

    public override void SetStorage(ChunkStorage newStorage) {
        isFilled = true;
        base.SetStorage(newStorage);
    }
}
