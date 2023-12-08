using GlmSharp;
using Voxel.Common.World;
using Voxel.Common.World.Storage;

namespace Voxel.Client.World;

public class ClientChunk : Chunk {

    public bool isFilled;

    public ClientChunk(ivec3 chunkPosition, VoxelWorld world, ChunkStorage? storage = null) : base(chunkPosition, world, storage) {

    }

    public override void SetStorage(ChunkStorage newStorage) {
        isFilled = true;
        base.SetStorage(newStorage);
    }
}
