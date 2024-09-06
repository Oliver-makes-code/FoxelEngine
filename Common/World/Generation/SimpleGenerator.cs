using System.Buffers;
using FastNoiseOO;
using Foxel.Common.Util;
using Foxel.Common.World.Content.Blocks;
using Foxel.Common.World.Storage;

namespace Foxel.Common.World.Generation;

public static class SimpleGenerator {
    private static readonly FastNoise Generator = FastNoise.FromEncodedNodeTree("JQAzMzM/AAAAPzMzMz8AAIA/KAARAAIAAAAAACBAEAAAAABAGQATAMP1KD8NAAQAAAAAACBACQAAZmYmPwAAAAA/AQQAAAAAAAAAQEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAzcxMPgAzMzM/AAAAAD8=");

    private static readonly ArrayPool<float> Pool = ArrayPool<float>.Create();

    public static void GenerateChunk(Chunk target) {

        var start = DateTime.Now;

        float[] noise = Pool.Rent(PositionExtensions.ChunkCapacity);
        var storage = new SimpleStorage(BlockStore.Blocks.Air.Get().DefaultState);

        var basePosition = target.WorldPosition;

        //NOTE - Swap X and Z positions because... Reasons?
        // Fastnoise always seems to argue over which axis is which, but this seems to be the correct result.
        Generator.GenUniformGrid3D(noise,
            basePosition.z, basePosition.y, basePosition.x,
            PositionExtensions.ChunkSize, PositionExtensions.ChunkSize, PositionExtensions.ChunkSize,
            0.05f, 1
        );

        for (int i = 0; i < noise.Length; i++) {
            float density = noise[i];

            if (density < 0)
                storage.SetBlock(BlockStore.Blocks.Stone.Get().DefaultState, i);
            else if (density < 0.2)
                storage.SetBlock(BlockStore.Blocks.Dirt.Get().DefaultState, i);
            else if (density < 0.3)
                storage.SetBlock(BlockStore.Blocks.Grass.Get().DefaultState, i);
        }

        storage.ReduceIfPossible(target, out var newStorage);
        target.SetStorage(newStorage);

        var end = DateTime.Now;
        //Console.Out.WriteLine($"Took {(end - start).TotalMilliseconds:##.##}ms to generate noise");

        Pool.Return(noise);
    }
}
