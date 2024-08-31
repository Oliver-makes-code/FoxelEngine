using GlmSharp;
using Foxel.Common.Util.Serialization;
using Foxel.Common.World;
using Foxel.Core;

namespace Foxel.Common.Network.Packets.S2C.Gameplay;

public class ChunkUnloadS2CPacket : S2CPacket {

    public ivec3 position { get; private set; }

    public void Init(Chunk target) {
        position = target.ChunkPosition;
    }

    public void Apply(Chunk target) {
        target.World.UnloadChunk(target.ChunkPosition);

        Game.Logger.Info($"Unload Chunk {target.ChunkPosition}");
    }

    public override void Write(VDataWriter writer) {
        writer.Write(position);
    }
    public override void Read(VDataReader reader) {
        position = reader.ReadIVec3();
    }
}
