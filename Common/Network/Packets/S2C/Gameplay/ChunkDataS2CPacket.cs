using GlmSharp;
using Voxel.Common.Content;
using Voxel.Common.Tile;
using Voxel.Common.Util.Serialization;
using Voxel.Common.World;
using Voxel.Common.World.Storage;

namespace Voxel.Common.Network.Packets.S2C.Gameplay;

public class ChunkDataS2CPacket : S2CPacket {

    public ivec3 position { get; private set; }
    private ChunkStorage storage;

    public void Init(Chunk chunk) {
        position = chunk.ChunkPosition;
        storage = chunk.storage.GenerateCopy();
    }

    public override void Write(VDataWriter writer) {
        writer.Write(position);

        //Console.WriteLine($"S: {position}");

        switch (storage) {
            case SingleStorage single: {
                writer.Write((int)Type.Single);
                writer.Write(single.Block.id);

                //Console.WriteLine($"Wrote Single Storage of block {single.Block.Name}");
                break;
            }
            case SimpleStorage simple: {
                writer.Write((int)Type.Simple);
                foreach (uint id in simple.BlockIds)
                    writer.Write(id);

                //Console.WriteLine($"Wrote Simple Storage");
                break;
            }
        }

        storage.Dispose();
    }


    public override void Read(VDataReader reader) {
        position = reader.ReadIVec3();

        //Console.WriteLine($"C: {position}");

        var type = (Type)reader.ReadInt();
        switch (type) {
            case Type.Single: {
                var rawID = reader.ReadUInt();
                if (!ContentDatabase.Instance.Registries.Blocks.RawToEntry(rawID, out var block))
                    throw new InvalidOperationException($"Could not read block from chunk data packet! Id was {rawID}");

                var single = new SingleStorage(block, null);
                storage = single;

                //Console.WriteLine($"Got Single Storage of block {single.Block.Name}");
                break;
            }
            case Type.Simple: {
                var simple = new SimpleStorage();
                storage = simple;

                for (var i = 0; i < simple.BlockIds.Length; i++)
                    simple.BlockIds[i] = reader.ReadUInt();

                //Console.WriteLine("Got Simple Storage");
                break;
            }
        }
    }

    public void Apply(Chunk chunk) {
        chunk.SetStorage(storage.WithChunk(chunk));
    }

    private enum Type : byte {
        Single,
        Simple,
    }
}
