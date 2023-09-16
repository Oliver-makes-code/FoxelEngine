using Microsoft.Xna.Framework;
using Voxel.Common.World;

namespace Voxel.Client.Rendering;

public class ChunkMesh {
    public static void BuildChunk(Chunk chunk, MeshBuilder builder) {
        for (byte y = 0; y < 0b10_0000u; y++) {
            for (byte x = 0; x < 0b10_0000u; x += 1) {
                for (byte z = 0; z < 0b10_0000u; z++) {
                    var block = chunk[false, x, y, z];
                    if (block != (ushort)0b0000_0000_0000_0000u) {
                        GenerateCube(builder, (byte)(x*2), (byte)(y*2), (byte)(z*2));
                    }
                }
            }
        }
    }

    public static void GenerateCube(MeshBuilder builder, byte x, byte y, byte z) {
        builder.Quad(
            new(x, y, z),
            new(x+1, y, z),
            new(x+1, y+1, z),
            new(x, y+1, z),
            Color.Red
        );

        builder.Quad(
            new(x, y, z+1),
            new(x, y+1, z+1),
            new(x+1, y+1, z+1),
            new(x+1, y, z+1),
            Color.Orange
        );

        builder.Quad(
            new(x, y+1, z),
            new(x+1, y+1, z),
            new(x+1, y+1, z+1),
            new(x, y+1, z+1),
            Color.Yellow
        );

        builder.Quad(
            new(x, y, z+1),
            new(x+1, y, z+1),
            new(x+1, y, z),
            new(x, y, z),
            Color.Green
        );

        builder.Quad(
            new(x, y, z),
            new(x, y+1, z),
            new(x, y+1, z+1),
            new(x, y, z+1),
            Color.Blue
        );

        builder.Quad(
            new(x+1, y, z+1),
            new(x+1, y+1, z+1),
            new(x+1, y+1, z),
            new(x+1, y, z),
            Color.Purple
        );
    }
}