using System;
using System.Collections.Generic;
using Voxel.Common.World;

namespace Voxel.Test.Tests;

class ChunkBlockPosSuite : TestSuite {
    protected override Dictionary<string, Test> DefineTests() => new() {
        ["Raw value to components"] = () => {
            var getRandomUShort = (Random rng) => { byte[] bytes = new byte[2]; rng.NextBytes(bytes); return (ushort)((bytes[0] << 8) | bytes[1]); };

            var getF = (ushort raw) => { return raw > 0b0111111111111111; };
            var getX = (ushort raw) => { return (raw & 0b0111110000000000) >> 10; };
            var getY = (ushort raw) => { return (raw & 0b0000001111100000) >> 5; };
            var getZ = (ushort raw) => { return (raw & 0b0000000000011111); };

            var rng = new Random(0);
            int iterations = 256;
            for (int i = 0; i < iterations; i++) {
                var raw = getRandomUShort(rng);
                var chunkPos = new ChunkTilePos(raw);

                var x = getX(raw);
                var y = getY(raw);
                var z = getZ(raw);

                Assert(getF(raw) == chunkPos.IsFluid, "extract fluid bit");
                Assert(getX(raw) == chunkPos.X, "extract X coordinate");
                Assert(getY(raw) == chunkPos.Y, "extract Y coordinate");
                Assert(getZ(raw) == chunkPos.Z, "extract Z coordinate");
            }
        },
        ["Components to raw value"] = () => {
            var composeID = (bool f, byte x, byte y, byte z) => { return ((f ? 1 : 0) << 15) | (x << 10) | (y << 5) | z; };

            var rng = new Random(0);
            int iterations = 256;
            for (int i = 0; i < iterations; i++) {
                bool f = rng.Next() % 2 == 0;   
                byte x = (byte)(rng.Next() & 0b11111);
                byte y = (byte)(rng.Next() & 0b11111);
                byte z = (byte)(rng.Next() & 0b11111);

                var chunkPos = new ChunkTilePos(f, x, y, z);

                Assert(composeID(f, x, y, z) == chunkPos.Raw, "Compose fluid, x, y, and z values into a ChunkPos raw id");
            }
        }
    };
}
