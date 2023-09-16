using Microsoft.Xna.Framework;
using Voxel.Common.World;

namespace Voxel.Client.Rendering;

public class ChunkMesh {
    public static void BuildChunk(Chunk chunk, MeshBuilder builder) {
        for (byte y = 0; y < 0b10_0000u; y++) {
            for (byte x = 0; x < 0b10_0000u; x += 1) {
                for (byte z = 0; z < 0b10_0000u; z++) {
                    GenerateCube(builder, chunk, x, y, z);
                }
            }
        }
    }

    public static void GenerateCube(MeshBuilder builder, Chunk chunk, byte x, byte y, byte z) {
        var block = chunk[false, x, y, z];
        if (block == 0)
            return;

        if (chunk[false, x, y, (byte)(z-1)] == 0) {
            builder.Quad(
                new(x, y, z),
                new(x+1, y, z),
                new(x+1, y+1, z),
                new(x, y+1, z),
                Color.Red
            );
        }

        if (chunk[false, x, y, (byte)(z+1)] == 0) {
            builder.Quad(
                new(x, y, z+1),
                new(x, y+1, z+1),
                new(x+1, y+1, z+1),
                new(x+1, y, z+1),
                Color.Orange
            );
        }

        if (chunk[false, x, (byte)(y+1), z] == 0) {
            builder.Quad(
                new(x, y+1, z),
                new(x+1, y+1, z),
                new(x+1, y+1, z+1),
                new(x, y+1, z+1),
                Color.Yellow
            );
        }

        if (chunk[false, x, (byte)(y-1), z] == 0) {
            builder.Quad(
                new(x, y, z+1),
                new(x+1, y, z+1),
                new(x+1, y, z),
                new(x, y, z),
                Color.Green
            );
        }

        if (chunk[false, (byte)(x-1), y, z] == 0) {
            builder.Quad(
                new(x, y, z),
                new(x, y+1, z),
                new(x, y+1, z+1),
                new(x, y, z+1),
                Color.Blue
            );
        }


        if (chunk[false, (byte)(x+1), y, z] == 0) {
            builder.Quad(
                new(x+1, y, z+1),
                new(x+1, y+1, z+1),
                new(x+1, y+1, z),
                new(x+1, y, z),
                Color.Purple
            );
        }
    }
}