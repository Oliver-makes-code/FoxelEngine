using GlmSharp;

namespace Foxel.Common.Util; 

public static class RandomExtensions {
    public static ivec3 NextChunkPos(this Random random)
        => new ivec3(random.Next(), random.Next(), random.Next()).Loop(PositionExtensions.ChunkSize);
}
