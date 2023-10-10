using System.Buffers;
using FastNoiseOO;
using Voxel.Common.Tile;
using Voxel.Common.Util;
using Voxel.Common.World.Storage;

namespace Voxel.Common.World.Generation;

public static class SimpleGenerator {

    private static FastNoise Generator = FastNoise.FromEncodedNodeTree("JQAzMzM/AAAAPzMzMz8AAIA/KAARAAIAAAAAACBAEAAAAABAGQATAMP1KD8NAAQAAAAAACBACQAAZmYmPwAAAAA/AQQAAAAAAAAAQEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAzcxMPgAzMzM/AAAAAD8=");

    private static ArrayPool<float> Pool = ArrayPool<float>.Create();

    public static void GenerateChunk(Chunk target) {

        var start = DateTime.Now;

        float[] noise = Pool.Rent(PositionExtensions.ChunkCapacity);
        var storage = new SimpleStorage(Blocks.Air);

        var basePosition = target.WorldPosition;

        //NOTE - Swap X and Z positions because... Reasons?
        // Fastnoise always seems to argue over which axis is which, but this seems to be the correct result.
        Generator.GenUniformGrid3D(noise,
            basePosition.z, basePosition.y, basePosition.x,
            PositionExtensions.ChunkSize, PositionExtensions.ChunkSize, PositionExtensions.ChunkSize,
            0.05f, 1
        );

        for (int i = 0; i < noise.Length; i++) {
            if (noise[i] < -0.6)
                storage[i] = Blocks.Stone;
            else if (noise[i] < -0.5) ;
            else if (noise[i] < -0.3)
                storage[i] = Blocks.Dirt;
            else if (noise[i] < -0.2) ;
            else if (noise[i] < 0)
                storage[i] = Blocks.Grass;
        }

        target.SetStorage(storage);

        var end = DateTime.Now;
        Console.Out.WriteLine($"Took {(end - start).TotalMilliseconds:##.##}ms to generate noise");

        Pool.Return(noise);
    }
}
