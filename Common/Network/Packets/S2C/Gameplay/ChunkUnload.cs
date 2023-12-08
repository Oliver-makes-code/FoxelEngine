using Common.Network.Packets.S2C;
using Common.Util.Serialization;
using GlmSharp;
using Voxel.Common.World;

namespace Voxel.Common.Network.Packets.S2C.Gameplay;

public class ChunkUnload : S2CPacket {

    public ivec3 position { get; private set; }

    public void Init(Chunk target) {
        position = target.ChunkPosition;
    }

    public void Apply(Chunk target) {
        target.World.UnloadChunk(target.ChunkPosition);
        
        Console.WriteLine($"Unload Chunk {target.ChunkPosition}");
    }

    public override void Write(VDataWriter writer) {
        writer.Write(position);
    }
    public override void Read(VDataReader reader) {
        position = reader.ReadIVec3();
    }
}
