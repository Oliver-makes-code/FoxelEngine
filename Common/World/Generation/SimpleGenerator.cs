using System.Buffers;
using FastNoiseOO;
using Foxel.Common.Content;
using Foxel.Common.Tile;
using Foxel.Common.Util;
using Foxel.Common.World.Storage;

namespace Foxel.Common.World.Generation;

public static class SimpleGenerator {

    private static FastNoise Generator = FastNoise.FromEncodedNodeTree("JQAzMzM/AAAAPzMzMz8AAIA/KAARAAIAAAAAACBAEAAAAABAGQATAMP1KD8NAAQAAAAAACBACQAAZmYmPwAAAAA/AQQAAAAAAAAAQEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAzcxMPgAzMzM/AAAAAD8=");

    private static ArrayPool<float> Pool = ArrayPool<float>.Create();

    public static void GenerateChunk(Chunk target) {

        var start = DateTime.Now;

        float[] noise = Pool.Rent(PositionExtensions.ChunkCapacity);
        var storage = new SimpleStorage(MainContentPack.Instance.Air);

        var basePosition = target.WorldPosition;

        //NOTE - Swap X and Z positions because... Reasons?
        // Fastnoise always seems to argue over which axis is which, but this seems to be the correct result.
        Generator.GenUniformGrid3D(noise,
            basePosition.z, basePosition.y, basePosition.x,
            PositionExtensions.ChunkSize, PositionExtensions.ChunkSize, PositionExtensions.ChunkSize,
            0.05f, 1
        );

        for (int i = 0; i < noise.Length; i++) {
            var density = noise[i];

            if (density < 0)
                storage.SetBlock(MainContentPack.Instance.Stone, i);
            else if (density < 0.2)
                storage.SetBlock(MainContentPack.Instance.Dirt, i);
            else if (density < 0.3)
                storage.SetBlock(MainContentPack.Instance.Grass, i);
        }

        storage.ReduceIfPossible(target, out var newStorage);
        target.SetStorage(newStorage);

        var end = DateTime.Now;
        //Console.Out.WriteLine($"Took {(end - start).TotalMilliseconds:##.##}ms to generate noise");

        Pool.Return(noise);
    }
}
