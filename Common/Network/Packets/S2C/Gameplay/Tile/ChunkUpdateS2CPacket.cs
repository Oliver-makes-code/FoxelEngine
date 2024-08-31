using GlmSharp;
using Foxel.Common.Util.Serialization;

namespace Foxel.Common.Network.Packets.S2C.Gameplay.Tile;

public class BlockChangedS2CPacket : S2CPacket {
    public struct Single {
        public ivec3 position;
        public uint blockId;
    }

    public dvec3 worldPos;
    public Single[] updates = [];

    public override void Write(VDataWriter writer) {
        writer.Write(worldPos);
        writer.Write(updates.Length);
        for (int i = 0; i < updates.Length; i++) {
            var update = updates[i];
            writer.Write(update.position);
            writer.Write(update.blockId);
        }
    }
    
    public override void Read(VDataReader reader) {
        worldPos = reader.ReadDVec3();
        int length = reader.ReadInt();
        updates = new Single[length];
        for (int i = 0; i < updates.Length; i++) {
            var pos = reader.ReadIVec3();
            var id = reader.ReadUInt();
            var update = new Single {
                position = pos,
                blockId = id
            };
            updates[i] = update;
        }
    }
}
